<script setup lang="ts">
import SliderCaptcha from "./components/SliderCaptcha.vue";
import { ref } from "vue";

const DomSliderCaptcha = ref();

const isRetrieveCaptchaImageSuccess = ref(false);

const targetUrl = "/api/SliderCaptcha";
const lang = "";
</script>

<template>
  <h1>Slider Captcha Example</h1>

  <div v-show="isRetrieveCaptchaImageSuccess">
    <SliderCaptcha
      ref="DomSliderCaptcha"
      :acceptablePixelRange="2"
      :targetUrl="targetUrl"
      :lang="lang"
      @UpdateRetrieveCaptchaImageStatus="isRetrieveCaptchaImageSuccess = $event"
    />
  </div>

  <div v-if="!isRetrieveCaptchaImageSuccess">
    <div>Sorry, can not retrieve captcha images :(</div>
    <div>
      Click here to retry →
      <button @click="DomSliderCaptcha.RetrieveImg">click me!</button>
    </div>
  </div>
</template>

<style lang="scss"></style>
