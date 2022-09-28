<script setup lang="ts">
import { computed, nextTick, onMounted, ref } from "vue";
import IconArrowRight from "./icons/IconArrowRight.vue";
import IconXmark from "./icons/IconXmark.vue";
import IconCheck from "./icons/IconCheck.vue";
import IconRotateRight from "./icons/IconRotateRight.vue";
import { GetParam } from "@/utils/getParam";
import { GetStandardDeviation } from "@/utils/GetStandardDeviation";

const CaptchaImgSlider = ref();
const CaptchaImgBackground = ref();

const SliderDragArea = ref();
const SliderDragAreaButton = ref();

type SliderCaptcha = {
  captchaDirection: CaptchaDirection;
  imageWidth: number;
  imageHeight: number;
  backgroundBase64String: string;
  sliderBase64String: string;
  slideOffset?: number;
};

enum CaptchaDirection {
  LeftToRight,
  RightToLeft,
  TopToBottom,
  BottomToTop,
}

enum Axis {
  Horizontal,
  Vertical,
}

const isRetrieveCaptchaImageSuccess = ref(false);

const selectedDirection = ref(CaptchaDirection[CaptchaDirection.LeftToRight]);
const selectedWidth = ref(280);
const selectedHeight = ref(150);

const sliderCaptchaObject = ref<SliderCaptcha | null>(null);

const sliderImageNaturalWidth = ref(0);
const sliderImageNaturalHeight = ref(0);
const backgroundImageNaturalWidth = ref(0);
const backgroundImageNaturalHeight = ref(0);

const originX = ref(0);
const originY = ref(0);
const moveX = ref(0);
const moveY = ref(0);

const dragTrailRecord = ref<number[]>([0, 100, 500, -80, 40]);

onMounted(() => {
  RetrieveImg();
});

const isSliderDraged = computed(() => {
  return !!dragTrailRecord.value.length;
});

const dragSliderMovePixel = computed(() => {
  let width = 0;
  if (moveX.value <= 0) {
    width = 0;
  } else if (
    moveX.value >
    SliderDragArea.value.clientWidth - SliderDragAreaButton.value.clientWidth
  ) {
    width =
      SliderDragArea.value.clientWidth - SliderDragAreaButton.value.clientWidth;
  } else {
    width = moveX.value;
  }

  return width;
});

const processImageSliderMoveStyle = computed(() => {
  const isExist =
    CaptchaImgSlider.value?.clientWidth != null ||
    CaptchaImgSlider.value?.clientHeight != null ||
    CaptchaImgBackground.value?.clientWidth != null ||
    CaptchaImgBackground.value?.clientHeight != null;

  if (!isExist) return {};

  const imgRemainSpace =
    CaptchaImgBackground.value?.clientWidth -
    CaptchaImgSlider.value?.clientWidth;

  const dragRemainSpace =
    SliderDragArea.value?.clientWidth - SliderDragAreaButton.value?.clientWidth;

  const result = (dragSliderMovePixel.value / dragRemainSpace) * imgRemainSpace;

  return {
    marginLeft: `${result}px`,
  };
});

const processProgressBarStyle = computed(() => {
  return {
    width: `${dragSliderMovePixel.value}px`,
  };
});

const caledSlideOffsetResult = computed(() => {
  const imgRemainSpace =
    backgroundImageNaturalWidth.value - sliderImageNaturalWidth.value;

  const dragRemainSpace =
    SliderDragArea.value?.clientWidth - SliderDragAreaButton.value?.clientWidth;

  const result = (dragSliderMovePixel.value / dragRemainSpace) * imgRemainSpace;

  return result;
});

const isDragTrailManual = computed(() => {
  if (dragTrailRecord.value.length === 0) return false;
  // The concept of use this method is Y axes should not possible always be zero when drag
  return GetStandardDeviation(dragTrailRecord.value) !== 0;
});

const currentDirection = computed(() => {
  return sliderCaptchaObject.value?.captchaDirection ===
    CaptchaDirection.LeftToRight ||
    sliderCaptchaObject.value?.captchaDirection === CaptchaDirection.RightToLeft
    ? Axis.Horizontal
    : Axis.Vertical;
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
      sliderCaptchaObject.value = jsonData;

      isRetrieveCaptchaImageSuccess.value = true;

      nextTick(() => {
        sliderImageNaturalWidth.value = CaptchaImgSlider?.value?.naturalWidth;
        sliderImageNaturalHeight.value = CaptchaImgSlider?.value?.naturalHeight;
        backgroundImageNaturalWidth.value =
          CaptchaImgBackground?.value?.naturalWidth;
        backgroundImageNaturalHeight.value =
          CaptchaImgBackground?.value?.naturalHeight;
      });
    })
    .catch((err) => {
      isRetrieveCaptchaImageSuccess.value = false;
      console.log("Error:", err);
    });
};

const Reload = () => {
  originX.value = 0;
  originY.value = 0;

  moveX.value = 0;
  moveY.value = 0;
  dragTrailRecord.value = [];

  RetrieveImg();
};

const MouseDownDragButton = (e: MouseEvent) => {
  if (isSliderDraged.value) return;

  originX.value = e.clientX;
  originY.value = e.clientY;

  const mouseMoveHandler = (e1: MouseEvent): void => {
    moveX.value = e1.clientX - originX.value;
    moveY.value = e1.clientY - originY.value;

    if (
      moveX.value < 0 ||
      moveX.value >
        SliderDragArea.value.clientWidth -
          SliderDragAreaButton.value.clientWidth
    )
      return;

    //log move trail for validation
    dragTrailRecord.value.push(Math.round(moveY.value));
  };

  const mouseUpHandler = (): void => {
    document.removeEventListener("mouseup", mouseUpHandler);
    document.removeEventListener("mousemove", mouseMoveHandler);
  };

  document.addEventListener("mousemove", mouseMoveHandler);
  document.addEventListener("mouseup", mouseUpHandler);
};
</script>

<template>
  <template v-if="isRetrieveCaptchaImageSuccess">
    <h1>Slider captcha example</h1>

    <div id="captcha-area" v-if="sliderCaptchaObject">
      <div id="captcha-image-area">
        <img
          id="captcha-img-slider"
          ref="CaptchaImgSlider"
          :src="sliderCaptchaObject.sliderBase64String"
          :style="processImageSliderMoveStyle"
        />
        <img
          id="captcha-img-background"
          ref="CaptchaImgBackground"
          :src="sliderCaptchaObject.backgroundBase64String"
        />
        <IconRotateRight class="reload-icon" @click="Reload()" />
      </div>

      <div
        id="captcha-drag-area"
        ref="SliderDragArea"
        :class="
          sliderCaptchaObject.captchaDirection ===
            CaptchaDirection.LeftToRight ||
          sliderCaptchaObject.captchaDirection === CaptchaDirection.RightToLeft
            ? 'horizontal'
            : 'vertical'
        "
      >
        <div id="progress-bar" :style="processProgressBarStyle"></div>
        <div
          ref="SliderDragAreaButton"
          id="captcha-drag-area-button"
          @mousedown="MouseDownDragButton($event)"
        >
          <IconArrowRight class="button-icon" />
        </div>
      </div>
    </div>

    <div id="tools-area">
      <h3>tools</h3>

      <div class="tools-content">
        <div class="direction-select-block">
          <span>Direction:</span>
          <input
            type="radio"
            id="LeftToRight"
            :value="CaptchaDirection[CaptchaDirection.LeftToRight]"
            v-model="selectedDirection"
          />
          <label for="LeftToRight">LeftToRight</label>

          <input
            type="radio"
            id="RightToLeft"
            :value="CaptchaDirection[CaptchaDirection.RightToLeft]"
            v-model="selectedDirection"
          />
          <label for="RightToLeft">RightToLeft</label>

          <input
            type="radio"
            id="TopToBottom"
            :value="CaptchaDirection[CaptchaDirection.TopToBottom]"
            v-model="selectedDirection"
          />
          <label for="TopToBottom">TopToBottom</label>

          <input
            type="radio"
            id="BottomToTop"
            :value="CaptchaDirection[CaptchaDirection.BottomToTop]"
            v-model="selectedDirection"
          />
          <label for="BottomToTop">BottomToTop</label>
        </div>

        <div class="size-select-block">
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

        <div>
          <button @click="Reload()">Reload</button>
        </div>

        <div>
          <button @click="Reload()">Fit</button>
          <button @click="Reload()">Expand</button>
        </div>
      </div>
    </div>
    <hr />

    <div>
      <h3>status</h3>

      <div>[slideOffset:{{ sliderCaptchaObject?.slideOffset }}]</div>
      <div>[caledResult:{{ caledSlideOffsetResult }}]</div>
      <br />
      <div>[sliderImageNaturalWidth:{{ sliderImageNaturalWidth }}]</div>
      <div>[sliderImageNaturalHeight:{{ sliderImageNaturalHeight }}]</div>
      <div>[backgroundImageNaturalWidth:{{ backgroundImageNaturalWidth }}]</div>
      <div>
        [backgroundImageNaturalHeight:{{ backgroundImageNaturalHeight }}]
      </div>
      <br />

      <div>[isDraged:{{ isSliderDraged }}]</div>
      <div>[originX:{{ originX }}]</div>
      <div>[originY:{{ originY }}]</div>
      <div>[moveX:{{ moveX }}]</div>
      <div>[moveY:{{ moveY }}]</div>

      <br />

      <div>[processBarStyle:{{ processProgressBarStyle }}]</div>
      <div>[processImageSliderMoveStyle:{{ processImageSliderMoveStyle }}]</div>

      <br />
      <div>
        [isVerifySuccess:{{ isDragTrailManual }}] (this shouldn't parsed by
        back-end, may cause memory leak problem)
      </div>
      <div>[dragTrailRecord:{{ dragTrailRecord }}]</div>
    </div>
  </template>

  <template v-else>
    <div>Sorry, can not retrieve captcha images :(</div>
    <div>
      Click here to retry → <button @click="RetrieveImg()">click me!</button>
    </div>
  </template>
</template>

<style lang="scss">
#captcha-area {
  display: flex;

  &.horizontal {
    flex-direction: column;
  }

  &.vertical {
    flex-direction: row;
  }

  #captcha-image-area {
    display: inline-block;
    position: relative;

    #captcha-img-background {
      object-fit: fill;
    }

    #captcha-img-slider {
      position: absolute;
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

  #captcha-drag-area {
    display: flex;
    user-select: none;

    box-sizing: border-box;

    width: 100%;
    height: 100%;

    &.horizontal {
      width: 40px;
    }

    &.vertical {
      height: 40px;
    }
    background-color: #f7f9fa;
    border-radius: 2px;
    border: 1px solid #e6e8eb;

    #captcha-drag-area-button {
      width: 100%;
      height: 100%;

      box-sizing: border-box;

      &:hover {
        background-color: rgb(255, 255, 255);
      }

      background: #fff;
      box-shadow: 0 0 3px rgb(0 0 0 / 30%);
      cursor: pointer;
      transition: 0.2s linear;
      border-radius: 2px;
      display: flex;
      align-items: center;
      justify-content: center;

      .button-icon {
        width: 50%;
        height: 50%;

        &.arrow {
        }

        &.x-mark {
        }

        &.check {
        }
      }
    }

    #progress-bar {
      width: 100%;
      height: 100%;
      background-color: rgb(0, 255, 136);
    }
  }
}

#tools-area {
  .tools-content {
    background-color: rgb(68, 68, 68);
    border-radius: 5px;
    padding: 10px;

    .direction-select-block {
      display: flex;
      align-items: center;

      input {
        margin: 0;
        margin-left: 10px;
      }
    }

    .size-select-block {
      background-color: rgb(68, 68, 68);
      border-radius: 5px;
      padding: 5px;

      display: flex;
      align-items: center;

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
}
</style>
