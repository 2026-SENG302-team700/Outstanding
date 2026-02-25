<script lang="ts">
	import 'bootstrap/dist/css/bootstrap.min.css';
	import '../app.css';
	interface Props {
		children?: import('svelte').Snippet;
	}

	let { children }: Props = $props();
</script>

<div class="container-fluid bg-light min-vh-100 py-4">
	{@render children?.()}
</div>
