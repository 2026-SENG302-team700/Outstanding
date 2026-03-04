<script lang="ts">
	import 'bootstrap/dist/css/bootstrap.min.css';
	import '../app.css';
	import Toast from '$lib/toast/toast.svelte';
	
	interface Props {
		children?: import('svelte').Snippet;
	}

	let { children }: Props = $props();
</script>

<Toast />
<div class="container-fluid bg-light min-vh-100 py-4">
	{@render children?.()}
</div>
