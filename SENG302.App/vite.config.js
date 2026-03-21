import { sveltekit } from '@sveltejs/kit/vite';
import { defineConfig, searchForWorkspaceRoot } from 'vite';

export default defineConfig({
	plugins: [sveltekit()],
	server: {
		port: 5173, // Development server port
		proxy: { // Development API proxy (i.e. requests to localhost:5173/api will be proxied to localhost:5000)
			'/api': {
				target: 'http://localhost:5000',
				changeOrigin: true,
				secure: false
			}
		},
		fs: {
			allow: [
       
        searchForWorkspaceRoot(process.cwd()),
        // your custom rules
        '/Users/Shutt/Documents/GitHub/team-700/node_modules/bootstrap-icons/font/fonts/bootstrap-icons.woff',
		'/Users/Shutt/Documents/GitHub/team-700/node_modules/bootstrap-icons/font/fonts/bootstrap-icons.woff2'
      ],
		}
	}
});
