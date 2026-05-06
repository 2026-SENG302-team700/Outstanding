<script lang="ts">
	import "bootstrap/dist/css/bootstrap.min.css";
	import "bootstrap-icons/font/bootstrap-icons.css";
	import "../app.css";
	import Toast from "$lib/toast/toast.svelte";
	import { onMount } from "svelte";
	import { page } from "$app/state";
	interface Props {
		children?: import("svelte").Snippet;
	}

	let { children }: Props = $props();

	// added to ensure we can make use of bootstrap js for things like dropdowns
	onMount(async () => {
		// @ts-ignore
		await import("bootstrap");
	});
</script>

<Toast />
{#key page.url.pathname}
	<div class="container-fluid bg-light min-vh-100 py-4">
		{@render children?.()}
	</div>
{/key}
