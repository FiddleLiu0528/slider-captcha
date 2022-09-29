<script setup lang="ts">
import { computed, ref } from "vue";
import { Direction } from "@/types/Direction";
import { Axes } from "@/types/Axes";

import IconArrowRight from "@/components/icons/IconArrowRight.vue";
import IconXmark from "@/components/icons/IconXmark.vue";
import IconCheck from "@/components/icons/IconCheck.vue";

const emit = defineEmits(["AddDragTrailRecord", "UpdateDragedDistanceRate"]);

const prop = defineProps<{
  direction: Direction;
  currentAxes: Axes;
  isDragTrailManual: boolean;
  isDragedOffsetAcceptable: boolean;
}>();

const DragBar = ref();
const DragButton = ref();

const barThicknessPixel = ref(40);
const calBarThicknessPixel = computed(() => `${barThicknessPixel.value}px`);

const buttonLengthPercentage = ref(20);
const calButtonLengthPercentage = computed(
  () => `${buttonLengthPercentage.value}%`
);
const originX = ref(0);
const originY = ref(0);

const movedX = ref(0);
const movedY = ref(0);

const isDraged = ref(false);

const buttonDragedPixel = computed(() => {
  if (!DragBar.value || !DragButton.value) return;

  let MaxDistanceWidth =
    DragBar.value.clientWidth - DragButton.value.clientWidth;
  let MaxDistanceHeight =
    DragBar.value.clientHeight - DragButton.value.clientHeight;

  if (prop.direction === Direction.LeftToRight) {
    if (movedX.value <= 0) return 0;
    else if (movedX.value > MaxDistanceWidth) return MaxDistanceWidth;
    return movedX.value;
  } else if (prop.direction === Direction.RightToLeft) {
    if (movedX.value <= -MaxDistanceWidth) return MaxDistanceWidth;
    else if (movedX.value > 0) return 0;
    return Math.abs(movedX.value);
  } else if (prop.direction === Direction.TopToBottom) {
    if (movedY.value <= 0) return 0;
    else if (movedY.value > MaxDistanceHeight) return MaxDistanceHeight;
    return movedY.value;
  } else if (prop.direction === Direction.BottomToTop) {
    if (movedY.value <= -MaxDistanceHeight) return MaxDistanceHeight;
    else if (movedY.value > 0) return 0;
    return Math.abs(movedY.value);
  }

  return 0;
});

const processProgressBarStyle = computed(() => {
  return {
    ...(prop.currentAxes === Axes.Horizontal && {
      width: `${buttonDragedPixel.value}px`,
    }),
    ...(prop.currentAxes === Axes.Vertical && {
      height: `${buttonDragedPixel.value}px`,
    }),
  };
});

const ExecuteDragButtonEvent = (e: MouseEvent) => {
  if (isDraged.value) return;

  originX.value = e.clientX;
  originY.value = e.clientY;

  const mouseMoveHandler = (e1: MouseEvent): void => {
    movedX.value = e1.clientX - originX.value;
    movedY.value = e1.clientY - originY.value;

    const dragRemainSpace =
      prop.currentAxes === Axes.Horizontal
        ? DragBar.value.clientWidth - DragButton.value.clientWidth
        : DragBar.value.clientHeight - DragButton.value.clientHeight;

    if (prop.direction === Direction.LeftToRight) {
      if (movedX.value < 0 || movedX.value > dragRemainSpace) return;

      emit("AddDragTrailRecord", Math.round(movedY.value));
      emit(
        "UpdateDragedDistanceRate",
        Math.abs(movedX.value / dragRemainSpace)
      );
    }

    if (prop.direction === Direction.RightToLeft) {
      if (movedX.value < -dragRemainSpace || movedX.value > 0) return;

      emit("AddDragTrailRecord", Math.round(movedY.value));
      emit(
        "UpdateDragedDistanceRate",
        Math.abs(movedX.value / dragRemainSpace)
      );
    }

    if (prop.direction === Direction.TopToBottom) {
      if (movedY.value < 0 || movedY.value > dragRemainSpace) return;

      emit("AddDragTrailRecord", Math.round(movedX.value));
      emit(
        "UpdateDragedDistanceRate",
        Math.abs(movedY.value / dragRemainSpace)
      );
    }

    if (prop.direction === Direction.BottomToTop) {
      if (movedY.value < -dragRemainSpace || movedY.value > 0) return;

      emit("AddDragTrailRecord", Math.round(movedX.value));
      emit(
        "UpdateDragedDistanceRate",
        Math.abs(movedY.value / dragRemainSpace)
      );
    }
  };

  const mouseUpHandler = (): void => {
    isDraged.value = true;

    document.removeEventListener("mouseup", mouseUpHandler);
    document.removeEventListener("mousemove", mouseMoveHandler);
  };

  document.addEventListener("mousemove", mouseMoveHandler);
  document.addEventListener("mouseup", mouseUpHandler);
};

const Reset = () => {
  isDraged.value = false;

  originX.value = 0;
  originY.value = 0;

  movedX.value = 0;
  movedY.value = 0;

  emit("UpdateDragedDistanceRate", 0);
};

defineExpose({
  Reset,
});
</script>

<template>
  <div
    id="drag-bar"
    ref="DragBar"
    :class="{
      horizontal: prop.currentAxes === Axes.Horizontal,
      vertical: prop.currentAxes === Axes.Vertical,
      'left-to-right': direction === Direction.LeftToRight,
      'right-to-left': direction === Direction.RightToLeft,
      'top-to-bottom': direction === Direction.TopToBottom,
      'bottom-to-top': direction === Direction.BottomToTop,
    }"
  >
    <div id="progress-bar" :style="processProgressBarStyle"></div>

    <div
      id="drag-button"
      ref="DragButton"
      :class="{
        horizontal: prop.currentAxes === Axes.Horizontal,
        vertical: prop.currentAxes === Axes.Vertical,
      }"
      @mousedown="ExecuteDragButtonEvent($event)"
    >
      <IconArrowRight
        v-if="isDraged === false"
        class="button-icon arrow"
        :class="{
          'left-to-right': direction === Direction.LeftToRight,
          'right-to-left': direction === Direction.RightToLeft,
          'top-to-bottom': direction === Direction.TopToBottom,
          'bottom-to-top': direction === Direction.BottomToTop,
        }"
      />
      <IconXmark
        v-if="
          isDraged &&
          (isDragTrailManual === false || isDragedOffsetAcceptable === false)
        "
        class="button-icon"
      />
      <IconCheck
        v-if="isDraged && isDragTrailManual && isDragedOffsetAcceptable"
        class="button-icon"
      />
    </div>
  </div>
</template>

<style lang="scss" scoped>
#drag-bar {
  display: flex;
  user-select: none;
  box-sizing: border-box;

  background-color: #f7f9fa;
  border-radius: 2px;
  border: 1px solid #e6e8eb;

  width: 100%;
  height: 100%;

  &.horizontal {
    height: v-bind(calBarThicknessPixel);
  }

  &.vertical {
    width: v-bind(calBarThicknessPixel);
  }

  &.left-to-right {
    flex-direction: row;
  }
  &.right-to-left {
    flex-direction: row-reverse;
  }
  &.top-to-bottom {
    flex-direction: column;
  }
  &.bottom-to-top {
    flex-direction: column-reverse;
  }

  #progress-bar {
    width: 100%;
    height: 100%;
    background-color: rgb(0, 255, 136);
  }

  #drag-button {
    box-sizing: border-box;

    display: flex;
    align-items: center;
    justify-content: center;

    width: 100%;
    height: 100%;

    background: #fff;
    box-shadow: 0 0 3px rgb(0 0 0 / 30%);
    transition: 0.2s linear;
    border-radius: 2px;
    cursor: pointer;

    &.horizontal {
      width: v-bind(calButtonLengthPercentage);
    }

    &.vertical {
      height: v-bind(calButtonLengthPercentage);
    }

    &:hover {
      background-color: rgb(255, 255, 255);
    }

    .button-icon {
      width: 50%;
      height: 50%;

      &.arrow {
        &.left-to-right {
        }

        &.right-to-left {
          transform: rotate(180deg);
        }

        &.top-to-bottom {
          transform: rotate(90deg);
        }

        &.bottom-to-top {
          transform: rotate(-90deg);
        }
      }

      &.x-mark {
      }

      &.check {
      }
    }
  }
}
</style>
