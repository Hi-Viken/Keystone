import { defineConfig, loadEnv } from 'vite'
import path from 'path'
import createVitePlugins from './vite/plugins'

// https://vitejs.dev/config/
export default defineConfig(({ mode, command }) => {
  const env = loadEnv(mode, process.cwd())

  const alias = {
    // 设置路径
    '~': path.resolve(__dirname, './'),
    // 设置别名
    '@': path.resolve(__dirname, './src')
  }
  // vue-i18n 使用 ESM 构建（比 CJS 更快）
  alias['vue-i18n'] = 'vue-i18n/dist/vue-i18n.esm-bundler.js'
  return {
    plugins: createVitePlugins(env, command === 'build'),
    // vue-i18n 编译时标志（消除警告）
    define: {
      __VUE_I18N_FULL_INSTALL__: true,
      __VUE_I18N_LEGACY_API__: false,
      __INTLIFY_PROD_DEVTOOLS__: false
    },
    // 预构建依赖（加速首次页面加载）
    optimizeDeps: {
      include: [
        'vue',
        'vue-router',
        'pinia',
        'axios',
        'element-plus',
        'element-plus/dist/locale/zh-cn',
        'element-plus/dist/locale/en',
        'element-plus/dist/locale/zh-tw',
        '@element-plus/icons-vue',
        'echarts',
        'nprogress',
        'js-cookie',
        'vue-i18n',
        'vue-clipboard3',
        '@microsoft/signalr',
        '@vueuse/core',
        'dayjs',
        'dayjs/locale/zh-cn',
        'crypto-js',
        'crypto-js/md5',
        'qs',
        'sortablejs',
        'fuse.js',
        'file-saver',
        'pinia-plugin-persistedstate',
        'vxe-table',
        'vxe-pc-ui',
        'vxe-pc-ui/lib/language/zh-CN',
        'highlight.js/lib/core',
        'highlight.js/lib/languages/javascript',
        'highlight.js/lib/languages/sql',
        'highlight.js/lib/languages/xml',
        'highlight.js/lib/languages/css',
        'highlight.js/lib/languages/json',
        'highlight.js/lib/languages/csharp',
        'highlight.js/lib/languages/bash',
        'highlight.js/lib/languages/markdown',
        'highlight.js/lib/languages/diff',
        'highlight.js/lib/languages/yaml',
        'highlight.js/lib/languages/typescript',
        'highlight.js/lib/languages/python',
        'highlight.js/lib/languages/java',
        'highlight.js/lib/languages/shell',
        'highlight.js/lib/languages/plaintext',
        'jsencrypt/bin/jsencrypt.min',
      ]
    },
    resolve: {
      // https://cn.vitejs.dev/config/#resolve-alias
      alias: alias,
      // 导入时想要省略的扩展名列表
      // https://cn.vitejs.dev/config/#resolve-extensions
      extensions: ['.mjs', '.js', '.ts', '.jsx', '.tsx', '.json', '.vue']
    },
    css: {
      devSourcemap: true //开发模式时启用
    },
    base: env.VITE_APP_ROUTER_PREFIX,
    // 打包配置
    build: {
      sourcemap: command === 'build' ? false : 'inline',
      outDir: 'dist', //指定输出目录
      assetsDir: 'assets', //指定静态资源存储目录(相对于outDir)
      chunkSizeWarningLimit: 2000, //Adjust the limit to your desired value in KB
      // 将js、css文件分离到单独文件夹
      rollupOptions: {
        output: {
          chunkFileNames: 'static/js/[name]-[hash].js',
          entryFileNames: 'static/js/[name]-[hash].js',
          assetFileNames: 'static/[ext]/[name]-[hash].[ext]'
        }
      }
    },
    // vite 相关配置
    server: {
      port: 8080,
      host: true,
      open: true,
      // 预热关键模块，减少首次页面加载延迟
      warmup: {
        clientFiles: [
          './src/main.js',
          './src/App.vue',
          './src/router/index.js',
          './src/permission.js',
          './src/store/modules/user.js',
          './src/store/modules/permission.js',
          './src/store/modules/settings.js',
          './src/store/modules/app.js',
          './src/layout/index.vue',
          './src/layout/components/Navbar.vue',
          './src/layout/components/Sidebar/index.vue',
          './src/views/index.vue',
          './src/views/login.vue',
          './src/i18n/index.js',
          './src/utils/request.js',
        ]
      },
      proxy: {
        // https://cn.vitejs.dev/config/#server-proxy
        '/dev-api': {
          target: env.VITE_APP_API_HOST,
          changeOrigin: true,
          rewrite: (path) => path.replace(/^\/dev-api/, '')
        },
        '/msghub': {
          target: env.VITE_APP_API_HOST,
          ws: true,
          rewrite: (path) => path.replace(/^\/msgHub/, '')
        }
      }
    }
  }
})
