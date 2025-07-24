<template>
  <div :class="['contain', 'center-flex']">
    <img class="background" :src="bg">
    <div :class="['glass-effect']">
      <div :class="['task-status', 'center-flex']">
        {{ task_info }}
      </div>
      <img class="icon" :src="face">
      <div :class="['tip', 'center']">
        Create by Mobot at {{ create_time }}
      </div>
    </div>
  </div>
</template>

<style scoped>
@font-face {
  font-family: 'GothFont';
  src: url('../assets/fonts/Goth.ttf') format('truetype');
}

@font-face {
  font-family: 'SegUIVar';
  src: url('../assets/fonts/SegUIVar.ttf') format('truetype');
}

@font-face {
  font-family: 'msyh';
  src: url('../assets/fonts/msyh.ttc') format('truetype');
}

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
  font-family: 'GothFont', 'SegUIVar', 'msyh';
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
import { ref,onMounted,nextTick } from 'vue'
import axios from 'axios'

// 获取当前路由信息
const route = useRoute()

// 获取路径参数
// const bg = computed(() => route.params.bg)
const bg = ref(require("../assets/TaskStatus/background.png"));
const face = ref(require("../assets/TaskStatus/icon.png"));
const task_info = ref(`读取中`);

const create_time = ref(formatDate(new Date(), 'Y-M-D H:m:s'));

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
  if (id == undefined) { return; }
  axios.get('http://localhost:5416?id=' + id)
    .then(response => {
      if (response == undefined) { return; }
      bg.value = response.data.background;
      face.value = response.data.iconBase64;
      task_info.value = response.data.text;
      nextTick(() => {
        window.appLoaded = true; // 模拟加载完成
      });
    })
    .catch(error => {
      console.error('请求出错：', error)
    })
});

</script>