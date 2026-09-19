<template>
	<div class="app-container">
		<!-- 服务器基本信息 -->
		<el-card class="box-card" shadow="hover">
			<div slot="header"><span>服务器信息</span></div>
			<el-row :gutter="20">
				<el-col :span="12">
					<el-descriptions title="基本信息" :column="1" border>
						<el-descriptions-item label="主机名">{{ serverInfo.hostname }}</el-descriptions-item>
						<el-descriptions-item label="IP">{{ serverInfo.ip }}</el-descriptions-item>
						<el-descriptions-item label="CPU">{{ serverInfo.cpu }}</el-descriptions-item>
						<el-descriptions-item label="内存">{{ formatBytes(serverInfo.ram) }}</el-descriptions-item>
					</el-descriptions>
				</el-col>
				<el-col :span="12">
					<el-descriptions title="系统信息" :column="1" border>
						<el-descriptions-item label="OS 名称">{{ serverInfo.os.name }}</el-descriptions-item>
						<el-descriptions-item label="版本">{{ serverInfo.os.version }}</el-descriptions-item>
						<el-descriptions-item label="内核">{{ serverInfo.os.kernel }}</el-descriptions-item>
						<el-descriptions-item label="启动模式">{{ serverInfo.os.boot_mode }}</el-descriptions-item>
					</el-descriptions>
				</el-col>
			</el-row>
		</el-card>

		<!-- CPU 使用率 -->
		<!-- CPU 使用率比例图 -->
		<el-card class="box-card" shadow="hover">
			<div slot="header" class="el-card_header"><span>CPU 使用率</span></div>

			<div class="cpu-container">
				<div v-for="(cpu, index) in cpuRates" :key="index" class="cpu-item">
					<el-progress type="dashboard" :percentage="cpu" :stroke-width="10" :color="getCpuColor(cpu)" />
					<div class="progress-name">
						{{ index === 0 ? "汇总" : `核心 ${index}` }}
					</div>
				</div>
			</div>
		</el-card>
		<el-card class="box-card" shadow="hover">
			<div slot="header"><span>CPU</span></div>
			<div ref="CpuChart" style="height: 300px;"></div>
		</el-card>



		<el-card class="box-card" shadow="hover">
			<div slot="header"><span>内存使用率</span></div>
			<div class="memory-item">
				<div class="title">内存使用率</div>
				<div class="content">
					<el-progress type="dashboard" :percentage="memUsageRate" :stroke-width="10" :color="memColor" />
				</div>
				<div class="footer">
					已使用 {{ formatBytes(memUsed) }} / 总共 {{ formatBytes(memTotal) }}
				</div>
			</div>
		</el-card>
		<el-card class="box-card" shadow="hover">
			<div slot="header"><span>内存</span></div>
			<div ref="ArmChart" style="height: 300px;"></div>
		</el-card>


		<el-card class="box-card" shadow="hover">
			<div slot="header" class="el-card_header"><span>磁盘</span></div>

			<div class="disk-container">
				<div v-for="(item, index) in diskRates" :key="index" class="disk-item">
					<!-- <div class="footer" v-if="index !== 0">{{ diskRates.length - 1 }} 核心</div> -->
					<div class="progress-name">
						<!-- {{ index === 0 ? "汇总" : `核心 ${index}` }} -->
						{{ item.name }}
					</div>
					<el-progress type="dashboard" :percentage="item.size" :stroke-width="10"
						:color="getCpuColor(item.size)" />

					<!-- <div class="footer">{{ diskRates.length - 1 }} 核心</div> -->
					<div class="footer">{{ item.total }}GB / {{ item.total - item.used }}GB</div>
				</div>
			</div>
		</el-card>


		<!-- 磁盘 I/O -->
		<el-card class="box-card" shadow="hover">
			<div slot="header"><span>磁盘 I/O</span></div>
			<div ref="diskChart" style="height: 300px;"></div>
		</el-card>

		<!-- 网络 I/O -->
		<el-card class="box-card" shadow="hover">
			<div slot="header"><span>网络 I/O</span></div>
			<div ref="netChart" style="height: 300px;"></div>
		</el-card>

	</div>
</template>
<script setup>
import { ref, computed, onMounted } from "vue";
import { ElMessage } from "element-plus";
import * as echarts from "echarts";


// -------------------- 初始化 -----------------------
onMounted(async () => {
	serverInfo.value = await fetchServerInfo();
	cpuRates.value = await fetchCpuRates();

	const mem = await fetchMemoryInfo();
	memTotal.value = mem.total; memUsed.value = mem.used;

	diskRates.value = await fetchDisRates();

	nextTick(() => {
		CpuChartLoad();
		ArmChartLoad();
		DiskChartLoad();
		NetworkChartLoad();
	});

});

onActivated(() => {
	nextTick(() => {
		if (CpuchartInstance) {
			CpuchartInstance.resize();
		}
		if (ArmchartInstance) {
			ArmchartInstance.resize();
		}
		if (diskchartInstance) {
			diskchartInstance.resize();
		}
		if (NetWorkchartInstance) {
			NetWorkchartInstance.resize();
		}
	});
});

// -------------------- 格式化 -----------------------
const formatBytes = value => {
	const size = Number(value);
	if (isNaN(size) || size <= 0) return "0 B";
	const units = ["B", "KB", "MB", "GB", "TB"];
	let index = 0, s = size;
	while (s >= 1024 && index < units.length - 1) { s /= 1024; index++; }
	return `${s.toFixed(2)} ${units[index]}`;
};
// -------------------- 服务器信息 --------------------
const serverInfo = ref({
	hostname: "", ip: "", cpu: "", ram: 0, disks: [],
	os: { name: "", version: "", kernel: "", boot_mode: "" }
});
const fetchServerInfo = () => new Promise(resolve => {
	setTimeout(() => {
		resolve({
			hostname: "vm-Debian-host",
			ip: "192.168.31.41",
			cpu: "13th Gen Intel(R) Core(TM) i5-13400",
			ram: 16 * 1024 * 1024 * 1024,
			disks: [{ device: "sda", size: 500 * 1024 * 1024 * 1024 }],
			os: { name: "Debian GNU/Linux", version: "13", kernel: "6.12.57+deb13-amd64", boot_mode: "BIOS" }
		});
	}, 500);
});
// -------------------- CPU --------------------
const cpuRates = ref([]);
const fetchCpuRates = () => new Promise(resolve => {
	setTimeout(() => {
		const cores = 5;
		const rates = [];
		rates.push(parseFloat((Math.random() * 100).toFixed(2))); // 汇总
		for (let i = 0; i < cores; i++) { rates.push(parseFloat((Math.random() * 100).toFixed(2))); }
		resolve(rates);
	}, 500);
});
const getCpuColor = rate => rate < 50 ? "#67C23A" : rate < 80 ? "#E6A23C" : "#ff0000";
// -------------------- cpu面积 --------------------
const CpuChart = ref(null);
let CpuchartInstance = null;
const CpuLabels = ref([]);//时间
const CpuData = ref([]);
const CpumaxPoints = 20;
const updateCpuData = () => {
	const rx = Number((Math.random() * 100).toFixed(2));  // CPU 使用率 0-100
	const now = new Date().toLocaleTimeString();

	// 上一次的值用于计算上升比例
	// 如果有历史数据，取最后一条并确保 value 是数字
	const prevItem = CpuData.value[CpuData.value.length - 1];
	const prev = prevItem ? Number(prevItem.value) : rx;

	const diff = Number((rx - prev).toFixed(2));  // 增加值
	const idle = Number((100 - rx).toFixed(2));   // 空闲占比


	CpuData.value.push({ value: rx, diff, idle });
	CpuLabels.value.push(now);

	if (CpuData.value.length > CpumaxPoints) {
		CpuData.value.shift();
		CpuLabels.value.shift();
	}

	CpuchartInstance && CpuchartInstance.setOption({
		tooltip: {
			trigger: 'axis',
			formatter: (params) => {
				const idx = params[0].dataIndex;
				const item = CpuData.value[idx];
				if (!item) return "";

				return `
时间：${CpuLabels.value[idx]}<br/>
使用率：${item.value}%<br/>
增加：${item.diff}%<br/>
空闲占比：${item.idle}%
`;
			}
		},
		legend: { data: ['使用率'] },
		grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true },
		xAxis: { type: 'category', boundaryGap: false, data: CpuLabels.value },
		yAxis: { type: 'value', name: 'CPU' },
		series: [
			{
				name: '使用率',
				type: 'line',
				stack: '总量',
				areaStyle: {},
				data: CpuData.value.map(v => v.value)
			}
		]
	});
};
const CpuChartLoad = () => {
	// 初始化 ECharts
	CpuchartInstance = echarts.init(CpuChart.value);
	CpuchartInstance.setOption({
		tooltip: { trigger: 'axis' },
		legend: { data: ['使用率'] },
		grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true },
		xAxis: { type: 'category', boundaryGap: false, data: CpuLabels.value },
		yAxis: { type: 'value', name: 'CPU' },
		series: [
			{ name: '使用率', type: 'line', stack: '总量', areaStyle: {}, data: CpuData.value },
		]
	});

	// ⭐绑定窗口 resize 事件
	window.addEventListener('resize', () => {
		CpuchartInstance && CpuchartInstance.resize();
	});
	setInterval(updateCpuData, 2000);
}
// -------------------- 内存 --------------------
const memTotal = ref(0);
const memUsed = ref(0);
const fetchMemoryInfo = () => new Promise(resolve => {
	setTimeout(() => {
		const total = 16 * 1024 * 1024 * 1024;
		const used = Math.floor(Math.random() * total);
		resolve({ total, used });
	}, 500);
});
const memUsageRate = computed(() => memTotal.value === 0 ? 0 : ((memUsed.value / memTotal.value) * 100).toFixed(2));
const memColor = computed(() => { const rate = memUsageRate.value; return rate < 50 ? "#67C23A" : rate < 80 ? "#E6A23C" : "#ff0000"; });


// -------------------- cpu面积 --------------------
const ArmChart = ref(null);
let ArmchartInstance = null;
const ArmnetLabels = ref([]);//时间
const ArmnetRxData = ref([]);
const ArmmaxPoints = 20;
const ArmupdateNetworkData = () => {
	const rx = Number((Math.random() * 100).toFixed(2));  // CPU 使用率 0-100
	const now = new Date().toLocaleTimeString();

	// 上一次的值用于计算上升比例
	// 如果有历史数据，取最后一条并确保 value 是数字
	const prevItem = ArmnetRxData.value[ArmnetRxData.value.length - 1];
	const prev = prevItem ? Number(prevItem.value) : rx;

	const diff = Number((rx - prev).toFixed(2));  // 增加值
	const idle = Number((100 - rx).toFixed(2));   // 空闲占比


	ArmnetRxData.value.push({ value: rx, diff, idle });
	ArmnetLabels.value.push(now);

	if (ArmnetRxData.value.length > ArmmaxPoints) {
		ArmnetRxData.value.shift();
		ArmnetLabels.value.shift();
	}

	ArmchartInstance && ArmchartInstance.setOption({
		tooltip: {
			trigger: 'axis',
			formatter: (params) => {
				const idx = params[0].dataIndex;
				const item = ArmnetRxData.value[idx];
				if (!item) return "";

				return `
时间：${ArmnetLabels.value[idx]}<br/>
使用率：${item.value}%<br/>
增加：${item.diff}%<br/>
空闲占比：${item.idle}%
`;
			}
		},
		legend: { data: ['使用率'] },
		grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true },
		xAxis: { type: 'category', boundaryGap: false, data: ArmnetLabels.value },
		yAxis: { type: 'value', name: 'CPU' },
		series: [
			{
				name: '使用率',
				type: 'line',
				stack: '总量',
				areaStyle: {},
				data: ArmnetRxData.value.map(v => v.value)
			}
		]
	});
};
const ArmChartLoad = () => {
	// 初始化 ECharts
	ArmchartInstance = echarts.init(ArmChart.value);
	ArmchartInstance.setOption({
		tooltip: { trigger: 'axis' },
		legend: { data: ['使用率'] },
		grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true },
		xAxis: { type: 'category', boundaryGap: false, data: ArmnetLabels.value },
		yAxis: { type: 'value', name: 'CPU' },
		series: [
			{ name: '使用率', type: 'line', stack: '总量', areaStyle: {}, data: ArmnetRxData.value },
		]
	});

	// ⭐绑定窗口 resize 事件
	window.addEventListener('resize', () => {
		ArmchartInstance && ArmchartInstance.resize();
	});
	setInterval(ArmupdateNetworkData, 2000);
}



const diskRates = ref([]);
const fetchDisRates = () => new Promise(resolve => {
	setTimeout(() => {
		const rates = [
			{ name: "/", total: 100, used: 60 },
			{ name: "/boot", total: 10, used: 2 },
			{ name: "/etc", total: 20, used: 15 },
			{ name: "/home", total: 50, used: 25 }
		];

		// 计算百分比
		rates.forEach(d => d.size = ((d.used / d.total) * 100).toFixed(2));

		resolve(rates);
	}, 500);
});


// -------------------- 网络 I/O --------------------
const diskChart = ref(null);
let diskchartInstance = null;
const diskReadData = ref([]);
const diskWriteData = ref([]);
const diskLabels = ref([]);
const diskmaxPoints = 20;

const updateDiskData = () => {
	const rx = (Math.random() * 100).toFixed(2);
	const tx = (Math.random() * 100).toFixed(2);
	const now = new Date().toLocaleTimeString();

	diskReadData.value.push(rx);
	diskWriteData.value.push(tx);
	diskLabels.value.push(now);

	if (diskReadData.value.length > diskmaxPoints) { diskReadData.value.shift(); diskWriteData.value.shift(); diskLabels.value.shift(); }

	diskchartInstance && diskchartInstance.setOption({
		xAxis: { data: diskLabels.value },
		series: [
			{ data: diskWriteData.value },
			{ data: diskReadData.value }
		]
	});
};
const DiskChartLoad = () => {
	// 初始化 ECharts
	diskchartInstance = echarts.init(diskChart.value);
	diskchartInstance.setOption({
		tooltip: { trigger: 'axis' },
		legend: { data: ['读 (MB/s)', '写 (MB/s)'] },
		grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true },
		xAxis: { type: 'category', boundaryGap: false, data: diskLabels.value },
		yAxis: { type: 'value', name: 'MB/s' },
		series: [
			{ name: '读 (MB/s)', type: 'line', stack: '总量', areaStyle: {}, data: diskReadData.value },
			{ name: '写 (MB/s)', type: 'line', stack: '总量', areaStyle: {}, data: diskWriteData.value }
		]
	});

	// ⭐绑定窗口 resize 事件
	window.addEventListener('resize', () => {
		diskchartInstance && diskchartInstance.resize();
	});
	setInterval(updateDiskData, 2000);
}

//网络 I/O----------------------------------------------
const netChart = ref(null);
let NetWorkchartInstance = null;
const netRxData = ref([]);
const netTxData = ref([]);
const netLabels = ref([]);
const maxPoints = 20;

const updateNetworkData = () => {
	const rx = (Math.random() * 100).toFixed(2);
	const tx = (Math.random() * 100).toFixed(2);
	const now = new Date().toLocaleTimeString();

	netRxData.value.push(rx);
	netTxData.value.push(tx);
	netLabels.value.push(now);

	if (netRxData.value.length > maxPoints) { netRxData.value.shift(); netTxData.value.shift(); netLabels.value.shift(); }

	NetWorkchartInstance && NetWorkchartInstance.setOption({
		xAxis: { data: netLabels.value },
		series: [
			{ data: netTxData.value },
			{ data: netRxData.value }
		]
	});
};

const NetworkChartLoad = () => {
	// 初始化 ECharts
	NetWorkchartInstance = echarts.init(netChart.value);
	NetWorkchartInstance.setOption({
		tooltip: { trigger: 'axis' },
		legend: { data: ['上传 (MB/s)', '下载 (MB/s)'] },
		grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true },
		xAxis: { type: 'category', boundaryGap: false, data: netLabels.value },
		yAxis: { type: 'value', name: 'MB/s' },
		series: [
			{ name: '上传 (MB/s)', type: 'line', stack: '总量', areaStyle: {}, data: netTxData.value },
			{ name: '下载 (MB/s)', type: 'line', stack: '总量', areaStyle: {}, data: netRxData.value }
		]
	});

	// ⭐绑定窗口 resize 事件
	window.addEventListener('resize', () => {
		NetWorkchartInstance && NetWorkchartInstance.resize();
	});
	setInterval(updateNetworkData, 2000);
}
</script>
<style>
.box-card {
	margin: 10px;
}

.col-item {
	text-align: center;
}

.title {
	margin-bottom: 10px;
	font-weight: bold;
}

.el-card_header {
	padding-bottom: 16px !important;
	/* 增加 header 下方间距 */
	margin-bottom: 12px !important;
	/* 再加一点外边距，让内容更舒服 */
	border-bottom: 1px solid #ebeef5;
	/* 可选：加条底线 */
}

.content {
	margin-bottom: 10px;
}

.cpu-container {
	display: flex;
	flex-wrap: wrap;
	/* 自动换行 */
	justify-content: center;
	/* 居中 */
	gap: 20px;
	/* 卡片之间的间距 */
	padding: 20px 0;
}

.cpu-item {
	width: 160px;
	/* CPU 卡片宽度 */
	text-align: center;
	/* background: #fff; */
	border-radius: 10px;
	padding: 15px;
	box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

.memory-item {
	text-align: center;
	margin-bottom: 20px;
}




.disk-container {
	display: flex;
	flex-wrap: wrap;
	/* 自动换行 */
	justify-content: center;
	/* 居中 */
	gap: 20px;
	/* 卡片之间的间距 */
	padding: 20px 0;
}

.disk-item {
	width: 160px;
	/* CPU 卡片宽度 */
	text-align: center;
	/* background: #fff; */
	border-radius: 10px;
	padding: 15px;
	box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

.progress-name {
	margin-bottom: 0px;
	font-weight: bold;
}
</style>