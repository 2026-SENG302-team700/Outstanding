<script lang="ts">
    // import defaultLogo from '$team-700/SENG302.App/static/defaultProfile.png/';
    import { onMount } from "svelte";
    import type { Book } from "$lib/types";
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
    import { fetchWithCsrf } from "$lib/csrf";
    import { countries } from "$lib/countries";
    
    
</script>

<div class="display: flex; flex-direction: row;">
    <button 
            class="btn btn-primary w-15"
            on:click={() => goto(resolve("/home"))}
    >
        Home
    </button>
    <div class="profile-box">
        <img class="profile-image", src="/defaultProfile.png" alt="No Profile Picture">
        <p class="username">Shivam</p>
        <p class="user_email">Email: panda.sickomode@gmail.com</p>
    </div>
</div>


<style>
    .cursor-pointer {
        cursor: pointer;
    }
    
    .profile-image {
        width: 20%;
        height: 20%;
        
        display: inline-block;
        object-fit: cover;
        border-radius: 50%;
    }
    
    .profile-box {
        display: flex;
        flex-direction: column;
        justify-content: center;
        align-items: center;
    }
    
    .username {
        margin-top: 20px;
        font-weight: 600;
        font-style: normal;
        font-size: 2rem;
        font-family: Arial;
    }
    
    .user_email {
        font-weight: 400;
        font-size: 1rem;
        font-family: Arial
    }
</style>