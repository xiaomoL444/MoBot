<template>
    <html>
    <meta name="referrer" content="no-referrer">

    </html>

    <div :class="['contain']">
        <div id="background" class="background" :style="{ '--main-color': dominantColor }">
            <img id="background-image" class="background-image" :src="bg" @load="OnImageLoad">
            <div id="background-pure-color" class="pure-color-background"></div>
        </div>

        <div :class="['account-flex-contain']" style="z-index: 2;">
            <div v-for="data in datas" :key="data" :class="['glass-effect', 'item']" style="overflow: hidden;">
                <div id="accountInfo" :class="['account-info']">
                    <img :class="['icon']" :src="data.icon" @load="OnImageLoad">
                    <div :class="['name']">{{ data.name }}</div>
                    <div v-html="data.info" :class="['info']"></div>
                </div>
            </div>
        </div>
        <TipLabel style="z-index: 999;"></TipLabel>
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
    height: auto;
    display: flex;
    flex-direction: column;
}


.background {
    position: absolute;
    width: 100%;
    height: 100%;
}

.background-image {
    position: absolute;
}

.pure-color-background {
    position: absolute;
    mask-image: linear-gradient(to bottom, transparent 30vw, black 50vw);
    width: 100%;
    height: 100%;
    background: var(--main-color);
    z-index: 1;
}

.account-flex-contain {
    flex: auto;
    display: flex;
    flex-direction: column;
    padding-left: 2vw;
    padding-right: 2vw;
    padding-top: 30vw;
    /* 留白 */
    box-sizing: border-box;
    gap: 1vw;
    width: 100%;
    height: 90%;
}

.account-info {
    position: relative;
    height: 100%;
    /* height: 11vw; */
    /* background-color: #00000050; */
}

.account-info>.icon {
    position: absolute;
    left: 2vw;
    top: 2vw;
    width: 8vw;
    height: 8vw;
    border-radius: 100%;
}

.account-info>.name {
    position: absolute;
    left: 12vw;
    top: 1.5vw;
    font-size: 4vw;
}

.account-info>.info {
    position: relative;
    height: 100%;
    box-sizing: border-box;
    padding-left: 12vw;
    padding-top: 7vw;
    padding-bottom: 1vw;
    /* left: 12vw; */
    /* top: 7vw; */
    font-size: 3vw;
    white-space: pre-wrap
}
</style>

<script setup>

import TipLabel from '@/components/TipLabel.vue';

import { useRoute } from 'vue-router'
import { ref, onMounted, nextTick } from 'vue'
import axios from 'axios'
import ColorThief from "colorthief";
// import ColorThief from './node_modules/colorthief/dist/color-thief.mjs'
// 获取当前路由信息
const route = useRoute()

const bg = ref('');
const datas = ref([{}]);

const dominantColor = ref('')
var image_load_count = 0;
var total_image_count = 0;

onMounted(() => {
    window.appLoaded = false;
    var id = route.query.id;
    console.log(id);
    console.log(datas.value);

    bg.value = require('../assets/image/HelpList/background.jpg');
    nextTick(() => {
        const img = document.getElementById('background-image');
        const colorThief = new ColorThief();
        // Make sure image is finished loading
        if (img.complete) {
            var a = colorThief.getColor(img);
            console.log(a);
            dominantColor.value = `rgb(${a})`;
            console.log(dominantColor.value);
        } else {
            img.addEventListener('load', function () {
                a = colorThief.getColor(img);
                console.log(a);
                dominantColor.value = `rgb(${a})`;
                console.log(dominantColor.value);
            });
        }

    });

    if (id == undefined) { showDefaultmsg(); return; }
    axios.get('http://mobot.lan:5416?id=' + id)
        .then(async response => {
            if (response == undefined) { return; }

            console.log(response.data);
            datas.value = response.data.account_infos;
            total_image_count = response.data.image_count;
        })
        .catch(error => {
            showDefaultmsg();
            console.error('请求出错：', error)
        })
});

async function OnImageLoad() {
    image_load_count++;
    console.log("image load:" + image_load_count);
    if (image_load_count == total_image_count) {
        console.log("load finished!");
        nextTick();
        console.log("Dom finished!")
        window.appLoaded = true;
    }
}

function showDefaultmsg() {
    datas.value = [{ name: "晓末L444", icon: require("../assets/image/AccountList/icon.png"), info: "已开通直播间\n123\n123" },
    { name: "Name", icon: require("../assets/image/AccountList/icon.png"), info: "这是一个介绍" }];
}
</script>