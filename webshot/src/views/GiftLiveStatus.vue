<template>
    <html>
    <meta name="referrer" content="no-referrer">

    </html>

    <div :class="['contain']">
        <img class="background" :src="bg" @load="OnImageLoad">
        <div :class="['flex-contain']">
            <div v-for="data in datas" :key="data" :class="['glass-effect', 'item']">
                <div :class="['task-status']">
                    {{ data.info }}
                </div>
                <img class="icon" :src="data.face" @load="OnImageLoad">
                <div :class="['tip', 'center']">
                    Create by Mobot at {{ create_time }}
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
@import '../assets/css/glass-effect.css';

img {
    width: 100%;
    height: auto;
}


.contain {
    height: 100vh;

}

.flex-contain {
    position: absolute;
    display: flex;
    padding: 10px;
    /* 留白 */
    box-sizing: border-box;
    gap: 1vw;
    width: 100%;
    height: 100%;
}

.item {
    flex: 1;
    width: 100%;
    height: auto;
    /* margin-left: 1vw; */
}

.background {
    position: absolute;
}

.task-status {
    position: relative;
    padding-left: 2vw;
    font-size: 2.35vw;
    /* 允许换行符生效 */

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

const bg = ref('');
const datas = ref([{}]);

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
            bg.value = response.data.background;
            datas.value = response.data.data;
        })
        .catch(error => {
            showDefaultmsg();
            console.error('请求出错：', error)
        })
});

function showDefaultmsg() {
    bg.value = require('../assets/GiftLiveStatus/background.jpg');
    datas.value = [{}, {}];
}

async function OnImageLoad() {
    image_load_count++;
    console.log("image load:" + image_load_count);
    if (image_load_count == Object.keys(datas.value).length + 1) {
        console.log("load finished!");
        nextTick();
        console.log("Dom finished!")
        window.appLoaded = true;
    }
}
</script>