<script setup lang="ts">
import { computed, nextTick, onMounted, ref } from "vue";
import DragBar from "@/components/widget/DragBar.vue";

import { Axes } from "@/types/Axes";
import { Direction } from "@/types/Direction";
import { DisplayMode } from "@/types/DisplayMode";

import IconRotateRight from "./icons/IconRotateRight.vue";
import { GetParam } from "@/utils/getParam";
import { GetStandardDeviation } from "@/utils/GetStandardDeviation";

const prop = defineProps<{
  targetUrl: string;
  lang: string;
}>();

const emit = defineEmits(["UpdateRetrieveCaptchaImageStatus"]);

const DomDragBar = ref();

const ImageSlider = ref();
const ImageBackground = ref();

type SliderCaptcha = {
  captchaDirection: Direction;
  imageWidth: number;
  imageHeight: number;
  backgroundBase64String: string;
  sliderBase64String: string;
  slideOffset?: number;
};

const isRetrieveCaptchaImageSuccess = ref(false);

const selectedDirection = ref(Direction[Direction.LeftToRight]);
const selectedWidth = ref(280);
const selectedHeight = ref(150);
const selectedDisplayMode = ref(DisplayMode.Exact);

const resultObject = ref<SliderCaptcha | null>(null);

const imageSliderNaturalWidth = ref(0);
const imageSliderNaturalHeight = ref(0);
const imageBackgroundNaturalWidth = ref(0);
const imageBackgroundNaturalHeight = ref(0);

const dragTrailRecord = ref<number[]>([]);

const dragedDistanceRate = ref(0);

onMounted(() => {
  SetDefault();
  RetrieveImg();
});

const SetDefault = () => {
  selectedDirection.value = Direction[Direction.LeftToRight];
  selectedWidth.value = 280;
  selectedHeight.value = 150;
  selectedDisplayMode.value = DisplayMode.Exact;
};

const currentAxes = computed(() => {
  return resultObject.value?.captchaDirection === Direction.LeftToRight ||
    resultObject.value?.captchaDirection === Direction.RightToLeft
    ? Axes.Horizontal
    : Axes.Vertical;
});

const processImageSliderStyle = computed(() => {
  const isExist =
    ImageSlider.value?.clientWidth != null &&
    ImageSlider.value?.clientHeight != null &&
    ImageBackground.value?.clientWidth != null &&
    ImageBackground.value?.clientHeight != null;

  if (!isExist) return {};

  const imgRemainSpace =
    currentAxes.value === Axes.Horizontal
      ? ImageBackground.value.clientWidth - ImageSlider.value.clientWidth
      : ImageBackground.value.clientHeight - ImageSlider.value.clientHeight;

  const result = dragedDistanceRate.value * imgRemainSpace;

  return {
    ...(resultObject.value?.captchaDirection === Direction.LeftToRight && {
      top: 0,
      left: `${result}px`,
    }),
    ...(resultObject.value?.captchaDirection === Direction.RightToLeft && {
      top: 0,
      right: `${result}px`,
    }),
    ...(resultObject.value?.captchaDirection === Direction.TopToBottom && {
      top: `${result}px`,
    }),
    ...(resultObject.value?.captchaDirection === Direction.BottomToTop && {
      bottom: `${result}px`,
    }),
  };
});

const processDisplayModeStyle = computed(() => {
  return currentAxes.value === Axes.Horizontal
    ? { height: "100%" }
    : { width: "100%" };
});

const dragedOffset = computed(() => {
  const imgRemainSpace =
    currentAxes.value === Axes.Horizontal
      ? imageBackgroundNaturalWidth.value - imageSliderNaturalWidth.value
      : imageBackgroundNaturalHeight.value - imageSliderNaturalHeight.value;

  return dragedDistanceRate.value * imgRemainSpace;
});

const isDragTrailManual = computed(() => {
  if (dragTrailRecord.value.length === 0) return false;
  // The concept of use this method is Y axes should not possible always be zero when drag
  return GetStandardDeviation(dragTrailRecord.value) !== 0;
});

const isDragedOffsetAcceptable = computed(() => {
  if (!resultObject.value?.slideOffset) return false;
  const acceptablePixelRange = 2; //

  return (
    Math.abs(dragedOffset.value - resultObject.value.slideOffset) <
    acceptablePixelRange
  );
});

const RetrieveImg = () => {
  const queryObject = {
    CaptchaDirection: selectedDirection.value,
    WidthAndHeight: {
      Width: selectedWidth.value,
      Height: selectedHeight.value,
    },
  };

  fetch(`/api/SliderCaptcha?${GetParam(queryObject).toString()}`)
    .then((response) => {
      return response.json();
    })
    .then((jsonData: SliderCaptcha) => {
      resultObject.value = jsonData;

      isRetrieveCaptchaImageSuccess.value = true;
      emit("UpdateRetrieveCaptchaImageStatus", true);

      nextTick(() => {
        imageSliderNaturalWidth.value = ImageSlider?.value?.naturalWidth;
        imageSliderNaturalHeight.value = ImageSlider?.value?.naturalHeight;
        imageBackgroundNaturalWidth.value =
          ImageBackground?.value?.naturalWidth;
        imageBackgroundNaturalHeight.value =
          ImageBackground?.value?.naturalHeight;
      });
    })
    .catch((err) => {
      isRetrieveCaptchaImageSuccess.value = false;
      emit("UpdateRetrieveCaptchaImageStatus", false);
      console.log("Error:", err);
    });
};

const Reload = () => {
  dragTrailRecord.value = [];
  DomDragBar.value.Reset();
  RetrieveImg();
};

defineExpose({
  RetrieveImg,
});
</script>

<template>
  <div id="captcha-area" v-if="resultObject">
    <div
      class="captcha-container"
      :class="[
        currentAxes === Axes.Horizontal
          ? 'layout-vertical'
          : 'layout-horizontal',
        { 'fit-width': selectedDisplayMode === DisplayMode.FitWidth },
      ]"
    >
      <div
        class="image-block"
        :class="{ 'fit-width': selectedDisplayMode === DisplayMode.FitWidth }"
      >
        <img
          ref="ImageBackground"
          class="image-background"
          :class="{ 'fit-width': selectedDisplayMode === DisplayMode.FitWidth }"
          :src="resultObject.backgroundBase64String"
        />
        <img
          ref="ImageSlider"
          class="image-slider"
          :src="resultObject.sliderBase64String"
          :style="{ ...processImageSliderStyle, ...processDisplayModeStyle }"
        />
        <IconRotateRight class="reload-icon" @click="Reload()" />
      </div>

      <DragBar
        ref="DomDragBar"
        :direction="resultObject.captchaDirection"
        :isDragTrailManual="isDragTrailManual"
        :currentAxes="currentAxes"
        :isDragedOffsetAcceptable="isDragedOffsetAcceptable"
        @AddDragTrailRecord="dragTrailRecord.push($event)"
        @UpdateDragedDistanceRate="dragedDistanceRate = $event"
      />
    </div>
  </div>

  <div id="tools-area">
    <h3>tools</h3>

    <div class="content">
      <div class="select-block">
        <span>Direction:</span>
        <input
          type="radio"
          id="LeftToRight"
          :value="Direction[Direction.LeftToRight]"
          v-model="selectedDirection"
        />
        <label for="LeftToRight">LeftToRight</label>

        <input
          type="radio"
          id="RightToLeft"
          :value="Direction[Direction.RightToLeft]"
          v-model="selectedDirection"
        />
        <label for="RightToLeft">RightToLeft</label>

        <input
          type="radio"
          id="TopToBottom"
          :value="Direction[Direction.TopToBottom]"
          v-model="selectedDirection"
        />
        <label for="TopToBottom">TopToBottom</label>

        <input
          type="radio"
          id="BottomToTop"
          :value="Direction[Direction.BottomToTop]"
          v-model="selectedDirection"
        />
        <label for="BottomToTop">BottomToTop</label>
      </div>

      <div class="select-block">
        <span>Size:</span>
        <input
          type="range"
          min="128"
          max="2048"
          step="1"
          v-model="selectedWidth"
        />
        <input type="text" v-model="selectedWidth" />
        <label>Width</label>
        <input
          type="range"
          min="128"
          max="2048"
          step="1"
          v-model="selectedHeight"
        />
        <input type="text" v-model="selectedHeight" />
        <label>Height</label>
        <button
          style="margin-left: 10px"
          @click="
            selectedWidth = 280;
            selectedHeight = 150;
          "
        >
          set default
        </button>
      </div>

      <div class="select-block">
        <button @click="Reload()">Reload</button>
      </div>

      <hr />

      <div class="select-block">
        <span>Display mode:</span>
        <input
          type="radio"
          id="ImageExact"
          :value="DisplayMode.Exact"
          v-model="selectedDisplayMode"
        />
        <label for="ImageExact">Exact</label>

        <input
          type="radio"
          id="ImageFitWidth"
          :value="DisplayMode.FitWidth"
          v-model="selectedDisplayMode"
        />
        <label for="ImageFitWidth">Fit Width</label>
      </div>
    </div>
  </div>

  <div id="status-area">
    <h3>status</h3>

    <div class="content">
      <div>[selectedDisplayMode:{{ DisplayMode[selectedDisplayMode] }}]</div>
      <br />

      <div>[captchaDirection:{{ resultObject?.captchaDirection }}]</div>
      <br />

      <div>[sliderImageNaturalWidth:{{ imageSliderNaturalWidth }}]</div>
      <div>[sliderImageNaturalHeight:{{ imageSliderNaturalHeight }}]</div>
      <div>[backgroundImageNaturalWidth:{{ imageBackgroundNaturalWidth }}]</div>
      <div>
        [backgroundImageNaturalHeight:{{ imageBackgroundNaturalHeight }}]
      </div>
      <br />

      <div>[slideOffset:{{ resultObject?.slideOffset }}]</div>
      <div>[dragedDistanceRate:{{ dragedDistanceRate }}]</div>
      <div>[dragedOffset:{{ dragedOffset }}]</div>
      <br />

      <div>
        [isDragTrailManual:{{ isDragTrailManual }}] (this shouldn't parsed by
        back-end, may cause some waste)
      </div>
      <div>[isDragedOffsetAcceptable:{{ isDragedOffsetAcceptable }}]</div>
      <div>[dragTrailRecord:{{ dragTrailRecord }}]</div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
#captcha-area {
  display: flex;

  .captcha-container {
    display: flex;

    &.fit-width,
    .fit-width {
      width: 100%;
    }

    &.layout-vertical {
      flex-direction: column;
    }

    &.layout-horizontal {
      flex-direction: row;
    }
  }

  .image-block {
    position: relative;

    .image-background {
      display: block;
      object-fit: fill;
    }

    .image-slider {
      position: absolute;
      display: block;
    }

    .reload-icon {
      position: absolute;
      top: 5px;
      right: 5px;
      width: 30px;
      cursor: pointer;
      transition: 0.1s linear;

      fill: rgba(255, 255, 255, 0.5);
      filter: drop-shadow(3px 3px 2px rgba(0, 0, 0, 0.8));

      &:hover {
        fill: rgba(255, 255, 255, 0.8);
        filter: drop-shadow(3px 5px 2px rgba(0, 0, 0, 0.4));
      }
    }
  }
}

#tools-area,
#status-area {
  .content {
    background-color: rgb(68, 68, 68);
    border-radius: 5px;
    padding: 10px;
  }
}

#tools-area {
  .select-block {
    display: flex;
    align-items: center;
    padding: 10px;

    input {
      margin: 0;
      margin-left: 10px;
    }

    input[type="text"] {
      width: 30px;
      border-radius: 5px;
    }
  }
}
</style>