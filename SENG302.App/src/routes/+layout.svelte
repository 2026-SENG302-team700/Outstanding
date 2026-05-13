<script lang="ts">
	import "bootstrap/dist/css/bootstrap.min.css";
	import "bootstrap-icons/font/bootstrap-icons.css";
	import "../app.css";
	import Toast from "$lib/toast/toast.svelte";
	import { onMount } from "svelte";
	import { page } from "$app/state";
	import { goto } from "$app/navigation";
	import { resolve } from "$app/paths";
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
	<div class="container-fluid bg-light min-vh-footer py-4">
		{@render children?.()}
	</div>
	<footer class="footerBar">
		<div class="footerContent">
			<button
				class="btn btn-secondary"
				onclick={() =>
					goto(resolve(`/policies/content-policy?from=${encodeURIComponent(page.url.pathname)}`))}
			>
				Content Policy
			</button>
			<button
				class="btn btn-secondary"
				onclick={() =>
					goto(resolve(`/policies/privacy-policy?from=${encodeURIComponent(page.url.pathname)}`))}
			>
				Privacy Policy
			</button>
		</div>
	</footer>
{/key}
	
<style>
	.min-vh-footer {
		min-height: 93vh;
	}
	
	.footerBar {
		margin-top: auto;
		background-color: white;
		height: 60px;
		box-shadow: 0 -2px 6px rgba(214, 213, 210, 0.75);
		border-radius: 10px;
		display: flex;
		align-items: center;
	}

	.footerContent {
		width: 100%;
		display: flex;
		gap: 12px;
		padding: 0 15px;
		align-items: center;
	}
</style>
