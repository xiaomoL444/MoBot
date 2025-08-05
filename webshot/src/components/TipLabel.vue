<template>
    <div :class="['tip-contain']">
        <div :class="['glass-effect', 'tip']">
            <div>
                Create by Mobot at {{ create_time }}
            </div>
        </div>
    </div>
</template>

<style scoped>
@import '../assets/css/glass-effect.css';

.tip-contain {
    /* position: absolute; */
    flex: auto;
    display: flex;
    padding: 1vw;
    /* 留白 */
    box-sizing: border-box;
    justify-content: center;
}

.tip {
    height: 4vw;
    width: 50vw;
    justify-content: center;
    align-items: center;
    /* position: relative; */
    font-size: 2vw;
    /* text-align: center; */
    display: flex;
    box-sizing: border-box;
}
</style>

<script setup>

import { ref } from 'vue'

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
</script>