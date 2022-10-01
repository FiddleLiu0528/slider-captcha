<script setup lang="ts">
import { computed, ref } from "vue";
import { Direction } from "@/types/Direction";
import { Axes } from "@/types/Axes";

import IconArrowRight from "@/components/icons/IconArrowRight.vue";
import IconXmark from "@/components/icons/IconXmark.vue";
import IconCheck from "@/components/icons/IconCheck.vue";

const emit = defineEmits(["AddDragTrailRecord", "UpdateDragedDistanceRate"]);

const props = defineProps<{
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

const movedPercentage = ref(0);

const isDraged = ref(false);

const processProgressBarStyle = computed(() => {
  const result = `${
    movedPercentage.value * (100 - buttonLengthPercentage.value)
  }%`;

  return {
    ...(props.currentAxes === Axes.Horizontal && {
      width: result,
    }),
    ...(props.currentAxes === Axes.Vertical && {
      height: result,
    }),
  };
});

const ExecuteDragButtonEvent = (e: MouseEvent | TouchEvent) => {
  if (isDraged.value) return;

  originX.value = e instanceof MouseEvent ? e.clientX : e.touches[0].clientX;
  originY.value = e instanceof MouseEvent ? e.clientY : e.touches[0].clientY;

  const touchMoveHandler = (e1: MouseEvent | TouchEvent): void => {
    movedX.value =
      e1 instanceof MouseEvent
        ? e1.clientX - originX.value
        : e1.touches[0].clientX - originX.value;
    movedY.value =
      e1 instanceof MouseEvent
        ? e1.clientY - originY.value
        : e1.touches[0].clientY - originY.value;

    const dragRemainSpace =
      props.currentAxes === Axes.Horizontal
        ? DragBar.value.clientWidth - DragButton.value.clientWidth
        : DragBar.value.clientHeight - DragButton.value.clientHeight;

    if (props.direction === Direction.LeftToRight) {
      if (movedX.value < 0 || movedX.value > dragRemainSpace) return;

      movedPercentage.value = Math.abs(movedX.value / dragRemainSpace);
      emit("AddDragTrailRecord", Math.round(movedY.value));
      emit("UpdateDragedDistanceRate", movedPercentage.value);
    }

    if (props.direction === Direction.RightToLeft) {
      if (movedX.value < -dragRemainSpace || movedX.value > 0) return;

      movedPercentage.value = Math.abs(movedX.value / dragRemainSpace);
      emit("AddDragTrailRecord", Math.round(movedY.value));
      emit("UpdateDragedDistanceRate", movedPercentage.value);
    }

    if (props.direction === Direction.TopToBottom) {
      if (movedY.value < 0 || movedY.value > dragRemainSpace) return;

      movedPercentage.value = Math.abs(movedY.value / dragRemainSpace);
      emit("AddDragTrailRecord", Math.round(movedX.value));
      emit("UpdateDragedDistanceRate", movedPercentage.value);
    }

    if (props.direction === Direction.BottomToTop) {
      if (movedY.value < -dragRemainSpace || movedY.value > 0) return;

      movedPercentage.value = Math.abs(movedY.value / dragRemainSpace);
      emit("AddDragTrailRecord", Math.round(movedX.value));
      emit("UpdateDragedDistanceRate", movedPercentage.value);
    }
  };

  const touchEndHandler = (): void => {
    isDraged.value = true;

    document.removeEventListener(
      e instanceof MouseEvent ? "mousemove" : "touchmove",
      touchMoveHandler
    );
    document.removeEventListener(
      e instanceof MouseEvent ? "mouseup" : "touchend",
      touchEndHandler
    );
  };

  document.addEventListener(
    e instanceof MouseEvent ? "mousemove" : "touchmove",
    touchMoveHandler
  );
  document.addEventListener(
    e instanceof MouseEvent ? "mouseup" : "touchend",
    touchEndHandler
  );
};

const Reset = () => {
  isDraged.value = false;

  originX.value = 0;
  originY.value = 0;

  movedX.value = 0;
  movedY.value = 0;

  movedPercentage.value = 0;

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
      horizontal: props.currentAxes === Axes.Horizontal,
      vertical: props.currentAxes === Axes.Vertical,
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
        horizontal: props.currentAxes === Axes.Horizontal,
        vertical: props.currentAxes === Axes.Vertical,
      }"
      @mousedown="ExecuteDragButtonEvent($event)"
      @touchstart.prevent="ExecuteDragButtonEvent($event)"
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
      min-width: v-bind(calButtonLengthPercentage);
    }

    &.vertical {
      height: v-bind(calButtonLengthPercentage);
      min-height: v-bind(calButtonLengthPercentage);
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
