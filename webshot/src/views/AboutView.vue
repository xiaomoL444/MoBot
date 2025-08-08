<template>
  <div class="container">
    <img src="../assets/image/TaskStatus/background.png" class="bg-img" />
    <div class="bottom-bar">我是贴在图片底部的元素</div>
  </div>
</template>

<style scoped>
.container {
  position: relative;
  width: 100%;
  /* 高度自动由图片决定 */
}

.bg-img {
  display: block;
  width: 100%;
  height: auto;
}

.bottom-bar {
  position: absolute;
  bottom: 0;
  left: 0;
  width: 100%;
  padding: 8px 12px;
  box-sizing: border-box;
  background-color: rgba(0, 0, 0, 0.5); /* 半透明遮罩 */
  color: white;
  text-align: center;
  height: 100%;
}
</style>