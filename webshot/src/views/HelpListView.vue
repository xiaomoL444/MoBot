<template>
    <html>
    <meta name="referrer" content="no-referrer">

    </html>

    <div :class="['contain']">
        <div id="background" class="background" :style="{ '--main-color': dominantColor }">
            <img id="background-image" class="background-image" :src="bg" @load="OnImageLoad">
            <div id="background-pure-color" class="pure-color-background"></div>
        </div>

        <div :class="['model-flex-contain']" style="z-index: 2;">
            <div v-for="model in datas" :key="model" :class="['glass-effect', 'item']" style="overflow: hidden;">
                <div id="ModelInfo" :class="['model-info']">
                    <img :class="['icon']" :src="model.icon" @load="OnImageLoad">
                    <div :class="['name']">{{ model.name }}</div>
                    <div :class="['description']">{{ model.description }}</div>
                </div>
                <div :class="['plugin-flex-content']">
                    <div v-for="plugin in model.plugin_infos" :key="plugin" :class="['plugin-info']">
                        <img :class="['icon']" :src="plugin.icon" @load="OnImageLoad">
                        <div :class="['name']">{{ plugin.name }}</div>
                        <div :class="['description']">{{ plugin.description }}</div>
                    </div>
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

.model-flex-contain {
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

.model-info {
    position: relative;
    height: 11vw;
    /* background-color: #00000050; */
}

.model-info>.icon {
    position: absolute;
    left: 2vw;
    top: 2vw;
    width: 8vw;
    height: 8vw;
    border-radius: 2vw;
}

.model-info>.name {
    position: absolute;
    left: 12vw;
    top: 1.5vw;
    font-size: 4vw;
}

.model-info>.description {
    position: absolute;
    left: 12vw;
    top: 7vw;
    font-size: 3vw;
}

.plugin-flex-content {
    display: flex;
    flex-wrap: wrap;
    flex-direction: row;
    /* padding-bottom: 3vw; */
    /* padding: 2vw; */
    /* 留白 */
    box-sizing: border-box;
}

.plugin-info {
    position: relative;
    display: flex;
    align-items: center;
    /* 垂直居中 */
    height: 8vw;
    flex: 0 33.333%;
    box-sizing: border-box;
}

/* 每行背景交替（每三个一组） */
.plugin-info:nth-child(6n+1)::before {
    content: "";
    position: absolute;
    height: 100%;
    width: 300%;
    background-color: #FFFFFF50;
    /* 浅色 */
}

.plugin-info:nth-child(3n+1)::after {
    content: "";
    position: absolute;
    top: 0;
    left: 0;
    height: 100%;
    width: 300%;
    background-image: linear-gradient(to right,
            transparent 33.33%,
            rgba(0, 0, 0, 0.2) 33.33%,
            rgba(0, 0, 0, 0.2) 33.6%,
            transparent 33.6%,

            transparent 66.66%,
            rgba(0, 0, 0, 0.2) 66.66%,
            rgba(0, 0, 0, 0.2) 66.9%,
            transparent 66.9%),
        linear-gradient(to top,
            transparent 99%,
            rgba(0, 0, 0, 0.2) 99%,
            rgba(0, 0, 0, 0.2) 100%,
            transparent 100%);
    z-index: 1;
    pointer-events: none;
}

.plugin-info>.name {
    position: absolute;
    font-size: 2.25vw;
    left: 7.5vw;
    bottom: 3.75vw;
}

.plugin-info>.description {
    font-size: 2vw;
    left: 8vw;
    top: 4.5vw;
    position: absolute;
    word-wrap: break-word;
    overflow-wrap: break-word;
    white-space: normal;
}

.plugin-info>.icon {
    position: absolute;
    left: 1.5vw;
    width: 5vw;
    height: 5vw;
    border-radius: 1vw;
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
            datas.value = response.data.model_infos;
            total_image_count = response.data.image_count + 1;
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
    datas.value = [{
        name: "test1", icon: require("../assets/image/HelpList/icon.jpg"), description: "这是一个介绍", plugin_infos: [{
            name: "plugin1",
            icon: require("../assets/image/HelpList/icon.jpg"),
            description: "pluginDes"
        }, {
            name: "plugin1",
            icon: require("../assets/image/HelpList/icon.jpg"),
            description: "这是一条超级超级超级超级超级长的简介"
        }, {
            name: "plugin1",
            icon: require("../assets/image/HelpList/icon.jpg"),
            description: "pluginDes"
        }, {
            name: "plugin1",
            icon: require("../assets/image/HelpList/icon.jpg"),
            description: "pluginDes"
        }]
    },
    {
        name: "test1", icon: require("../assets/image/HelpList/icon.jpg"), description: "这是一个介绍", plugin_infos: [{
            name: "plugin1",
            icon: require("../assets/image/HelpList/icon.jpg"),
            description: "pluginDes"
        }, {
            name: "plugin1",
            icon: require("../assets/image/HelpList/icon.jpg"),
            description: "这是一条超级超级超级超级超级长的简介"
        }, {
            name: "plugin1",
            icon: require("../assets/image/HelpList/icon.jpg"),
            description: "pluginDes"
        }, {
            name: "plugin1",
            icon: require("../assets/image/HelpList/icon.jpg"),
            description: "pluginDes"
        }]
    }];
}
</script>