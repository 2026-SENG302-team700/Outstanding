import adapter from '@sveltejs/adapter-static';
import { vitePreprocess } from '@sveltejs/vite-plugin-svelte';

/** @type {import('@sveltejs/kit').Config} */
const config = {
	// handles our ts scripts i.e. <script lang="ts">
	preprocess: vitePreprocess(),
	kit: {
		adapter: adapter({
			fallback: 'index.html',
		}),
		paths: {
			// Specify the base path (for deployment since we are serving behind a reverse proxy, more information in sprint 2)
			base: process.env.NODE_ENV === 'production' ? '/prod' : process.env.NODE_ENV === 'test' ? '/test' : '',
		}
	}
};

export default config;
