<script lang="ts">
    import { goto } from "$app/navigation";
    import { resolve } from "$app/paths";
</script>

<div class="container py-4">
    <div class="card shadow-sm p-4">

        <h1 class="mb-3">Privacy Policy</h1>

        <p>
            content...
        </p>

        <hr />

        <h1>Cookie Management</h1>

        <p>
            OUTSTANDING uses cookies. Cookies are little pieces of data stored on your computer, placed there by a website.   
        </p>
        <p>
            OUTSTANDING's cookies are used to verify who you are every time you open the site.
            Without cookies, you would have to log into the site every single time you open it.
            Our cookies store your identifiers in a signed, encrypted format which communicates with our services to verify your identity, so that we
            can ensure that you are authorised to do perform all of the tasks you can do with our service.
            We also store a "CSRF" token on your computer, which helps prevent malicious 
            attackers from using your unique cookie from attacking our services by masquerading as you, and all cookies that we use are transmitted over 
            secure HTTPS connections.
        </p>
        <p>
            7 days after logging in, your cookies will expire and you will need to login again to continue use of the service, 
            regardless of if you used OUTSTANDING or not in this time period.
        </p>
        <p>
            OUTSTANDING does not use third party cookies.
        </p>

        <hr />

        <button
                class="btn btn-primary mt-3"
                onclick={() => goto(resolve("/home"))}
        >
            Back to Home
        </button>

    </div>
</div>

<style>
    h1, h4, h5 {
        font-weight: 600;
    }

    ul {
        margin-bottom: 1rem;
    }

    .card {
        border-radius: 12px;
    }
</style>