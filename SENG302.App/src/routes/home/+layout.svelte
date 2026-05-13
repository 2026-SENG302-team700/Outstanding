<script lang="ts">
  import { onMount } from "svelte";
  import { goto } from "$app/navigation";
  import { resolve } from "$app/paths";
  import { fetchWithCsrf } from "$lib/csrf";
  import { user } from "$lib/stores/user";
  import ProfilePic from "$lib/profilepic/profilepic.svelte";

  let pfpUrl: string | null = null;

  interface Props {
    children?: import("svelte").Snippet;
  }
  let { children }: Props = $props();

  onMount(() => {
    retrieveUser();
  });

  /**
   * Retrieves user information from backend.
   */
  async function retrieveUser() {
    try {
      const response = await fetchWithCsrf(`/api/user`, {
        method: "GET",
        credentials: "include",
      });

      const data = await response.json();
      if (!response.ok) {
        goto(resolve("/"));
        return;
      }

      user.set(data);

      if (data.profilePicture !== 0) {
        const pfpResponse = await fetchWithCsrf("/api/user/pfp", {
          method: "GET",
          credentials: "include",
        });
        const blob = await pfpResponse.blob();
        user.update((u) => ({
          ...u,
          pfpData: {
            imageSource: URL.createObjectURL(blob),
            offsetX: parseFloat(
              pfpResponse.headers.get("profile-offset-x") ?? "0",
            ),
            offsetY: parseFloat(
              pfpResponse.headers.get("profile-offset-y") ?? "0",
            ),
            zoom: parseFloat(
              pfpResponse.headers.get("profile-offset-zoom") ?? "1",
            ),
          },
        }));
      }
    } catch (err) {
      goto(resolve("/"));
    }
  }
</script>

<nav class="navBar">
  <div
    style="display: flex; flex-direction: row; align-items: center; padding-top: 5px; "
  >
    <div style="flex: 1;"></div>

    <div style="display: flex; gap: 7px;">
      <button
        class="btn btn-primary w-15"
        onclick={() => goto(resolve("/home"))}
        style="margin-left: 7px;"
      >
        Home
      </button>
      <button
        class="btn btn-secondary w-15"
        onclick={() => goto(resolve("/home/board"))}
        style="margin-left: 7px;"
      >
        View Board
      </button>
    </div>

    <div
      style="display: flex; flex-direction: row; align-items: center; flex: 1; justify-content: flex-end; margin-right: 5px;"
    >
      <p
        style="font-size: 18px; vertical-align: bottom; margin-bottom: 0px; padding-bottom: 0px; font-weight: 500;"
      >
        {$user.displayName}
      </p>
      <div
        class="profile-button"
        onclick={() => goto(resolve("/home/profile"))}
      >
        <ProfilePic pfpData={$user.pfpData} size="small" />
      </div>
    </div>
  </div>
</nav>

<div class="page-wrapper container-fluid bg-light py-4">
  <div class="content">
    {@render children?.()}
  </div>

  <footer class="footerBar">
    <div class="footerContent">
      <button
        class="btn btn-secondary"
        onclick={() => goto(resolve("/home/content-policy"))}
      >
        Content Policy
      </button>
      <button class="btn btn-secondary" onclick={() => goto(resolve("/home/privacy-policy"))}>
        Privacy Policy
      </button>
    </div>
  </footer>
</div>

<style>
  .profile-image {
    width: 100%;
    height: 100%;
    object-fit: cover;
    border-radius: 50%;
    border-color: black;
  }

  .profile-button {
    width: 40px;
    height: 40px;
    padding: 0;
    background: none;
    border-radius: 50%;
    margin-left: 5px;
    cursor: pointer;
  }

  .navBar {
    top: 0;
    z-index: 1000;
    vertical-align: top;
    background-color: white;
    height: 50px;
    margin-top: 0px;
    padding-top: 0px;
    box-shadow:
      1px 5px 4px rgba(214, 213, 210, 0.75),
      -1px -5px 4px rgba(214, 213, 210, 0.75);
    border-radius: 10px;
    margin-bottom: 20px;
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

  .page-wrapper {
    min-height: 85vh;
    display: flex;
    flex-direction: column;
  }
</style>
