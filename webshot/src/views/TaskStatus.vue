<template>
  <html>
  <meta name="referrer" content="no-referrer">
  </html>
  <div :class="['contain', 'center-flex']">
    <img class="background" :src="img_list.bg" @load="OnImageLoad">
    <div :class="['glass-effect']">
      <div :class="['task-status', 'center-flex']">
        {{ task_info }}
      </div>
      <img class="icon" :src="img_list.face" @load="OnImageLoad">
      <div :class="['tip', 'center']">
        Create by Mobot at {{ create_time }}
      </div>
    </div>
  </div>
</template>

<style scoped>
/* .font-family{
  font-family: 'GothFont','SegUIVar','msyh';
} */

img {
  width: 100%;
  height: auto;
}

.center-flex {
  display: flex;
  justify-content: center;
  align-items: center;
}

.contain {
  height: 100vh;
  /* 使父容器的高度占满整个视口 */
}

.background {
  position: absolute;
}

.glass-effect {
  position: absolute;
  width: 95%;
  height: 91.6%;
  display: flex;
  background-color: rgba(255, 255, 255, 0.3921);
  /* 半透明白色背景 */
  backdrop-filter: blur(10px);
  /* 高斯模糊 */
  border-radius: 35px;
}

.task-status {
  position: relative;
  padding-left: 2vw;
  font-size: 2.35vw;
  /* 允许换行符生效 */
  white-space: pre;
}

.icon {
  position: absolute;
  top: 1.5vw;
  right: 1.5vw;
  width: 10vw;
  height: 10vw;

  border-radius: 100%;
}

.tip {
  position: absolute;
  bottom: 3vw;
  right: 3vw;
  font-size: 2vw;
}
</style>

<script setup>

import { useRoute } from 'vue-router'
import { ref, onMounted, nextTick } from 'vue'
import axios from 'axios'

// 获取当前路由信息
const route = useRoute()

// 获取路径参数
// const bg = computed(() => route.params.bg)
const img_list = ref({
  bg: '',
  face: ''
})
var isChange = false;
const task_info = ref('');

const create_time = ref(formatDate(new Date(), 'Y-M-D H:m:s'));

var image_load_count = 0;

function formatDate(date, format) {
  const map = {
    'Y': date.getFullYear(),          // 年
    'M': String(date.getMonth() + 1).padStart(2, '0'),  // 月，注意 JavaScript 中的月份从 0 开始，所以加 1
    'D': String(date.getDate()).padStart(2, '0'),      // 日
    'H': String(date.getHours()).padStart(2, '0'),     // 时
    'm': String(date.getMinutes()).padStart(2, '0'),   // 分
    's': String(date.getSeconds()).padStart(2, '0'),   // 秒
  };

  return format.replace(/Y|M|D|H|m|s/g, (match) => map[match]);
}
onMounted(() => {
  window.appLoaded = false;
  var id = route.query.id;
  console.log(id);
  if (id == undefined) { showDefaultmsg(); return; }
  axios.get('http://localhost:5416?id=' + id)
    .then(async response => {
      if (response == undefined) { return; }
      img_list.value.bg = response.data.background;
      img_list.value.face = response.data.face;
      isChange = true;
      task_info.value = response.data.text;
    })
    .catch(error => {
      showDefaultmsg();
      console.error('请求出错：', error)
    })
});

function showDefaultmsg() {
  img_list.value.bg = require("../assets/TaskStatus/background.png");
  img_list.value.face = require("../assets/TaskStatus/icon.png");
  task_info.value = '读取中';
}

async function OnImageLoad() {
  if (isChange == false) return;
  image_load_count++;
  console.log("image load:" + image_load_count);
  if (image_load_count == Object.keys(img_list.value).length) {
    console.log("load finished!");
    nextTick();
    console.log("Dom finished!")
    window.appLoaded = true;
  }
}
</script>