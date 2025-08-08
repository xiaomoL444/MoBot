<template>
    <html>
    <meta name="referrer" content="no-referrer">

    </html>

    <div :class="['contain']">
        <img class="background" :src="bg" @load="OnImageLoad">
        <div :class="['flex-contain']">
            <div v-for="extra_info in extra_infos" :key="extra_info">
                <div :class="['glass-effect', 'extra-info']">
                    {{ extra_info }}
                </div>
            </div>
            <div :class="['taskinfo-contain']">
                <div v-for="data in datas" :key="data" :class="['glass-effect', 'item']">
                    <div :class="['name']">{{ data.name }}</div>
                    <div v-html="data.info" :class="['task-status']"></div>
                    <img class="icon" :src="data.face" @load="OnImageLoad">

                </div>
            </div>

            <TipLabel></TipLabel>

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
    position: relative;
    /* display: flex; */
    height: auto;
    width: 100%;
    /* flex-direction: column; */
}

.extra-info {
    padding: 0.5vw;
    padding-left: 1.5vw;
    box-sizing: border-box;
    font-size: 1.5vw;
}

.flex-contain {
    position: absolute;
    top: 0;
    display: flex;
    padding: 2vw;
    /* 留白 */
    box-sizing: border-box;
    gap: 1vw;
    width: 100%;
    height: 100%;
    flex-direction: column;
}

.taskinfo-contain {
    position: relative;
    flex: auto;
    display: flex;
    /* padding: 2vw; */
    /* 留白 */
    box-sizing: border-box;
    gap: 1vw;
    height: 100%;
}

.item {
    flex: 1;
    width: 100%;
    height: auto;
    /* margin-left: 1vw; */
}

.background {
    /* position: absolute; */
    display: block;
    width: 100%;
    height: auto;
}

.name {
    padding-top: 2vw;
    position: relative;
    padding-left: 2vw;
    font-size: 2.75vw;
}

.task-status {
    position: relative;
    padding-top: 2vw;
    padding-left: 2vw;
    font-size: 2.35vw;
    /* 允许换行符生效 */
    white-space: pre-line;
    word-wrap: break-word;
}

.icon {
    position: absolute;
    top: 1.5vw;
    right: 1.5vw;
    width: 8vw;
    height: 8vw;

    border-radius: 100%;
}
</style>

<script setup>

import TipLabel from '@/components/TipLabel.vue'

import { useRoute } from 'vue-router'
import { ref, onMounted, nextTick } from 'vue'
import axios from 'axios'

// 获取当前路由信息
const route = useRoute()

const bg = ref('');
const datas = ref([{}]);
const extra_infos = ref([]);
var image_load_count = 0;

onMounted(() => {
    window.appLoaded = false;
    var id = route.query.id;
    console.log(id);
    if (id == undefined) { showDefaultmsg(); return; }
    axios.get('http://mobot.lan:5416?id=' + id)
        .then(async response => {
            if (response == undefined) { return; }
            // bg.value = response.data.background;
            bg.value = require('../assets/image/MultiInfoView/background.jpg');
            console.log(response.data);
            extra_infos.value = response.data.extra_infos;
            datas.value = response.data.data;
        })
        .catch(error => {
            showDefaultmsg();
            console.error('请求出错：', error)
        })
});

function showDefaultmsg() {
    bg.value = require('../assets/image/MultiInfoView/background.jpg');
    datas.value = [{
        name: "test",
        face: require('../assets/image/MultiInfoView/face.jpg'),
        info: "<img src='data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAoAAAAKCAIAAAACUFjqAAAACXBIWXMAAA7EAAAOxAGVKw4bAAABQUlEQVQYVwE2Acn+Af39/d+7+QchBgwW8+beD9HG5CY/EsHG9nFmFAAAAAHg2v7+/gD7APoFAAQLAwL+Bv9MYd5uWCQXE/LyBQIB29v17NHi/vEPHEEV8tX55svkHi0I+xsMzMDY3xM5AenQ5XqCkXtlgPn1/QABAiRBANW5AMG128YF1ngjCQGHSM1cei7s0gTc/Nagr75XMFsiO/3VxfDy8gQN+awBb1TX4d3jJjT7EDoqEtcF/gD/LScRxdPc29/gAx8GAP316ubg1EE5hP3r5/rv6cmtxkMmnKBq2ri3+8i43wC6r8/++Oz6xtP29Of+v7pKM6XRrPKLYuLOzPvMyO0EAxEw/gHrBDIZi3bu08vdc19Xxc3xKFQb893Q4dcHAUdjt599LdPJ6URMHca07AkKBfPy+fb0Avn3BLzcu3aeq9PM+GjnAAAAAElFTkSuQmCC' style='padding-left:2vw; vertical-align: middle; width: 3vw;'/><span style='vertical-align: middle;'>[长夜月_秋]正在观看直播</span>"
    }, {}];
    extra_infos.value = ["1", "2"];
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