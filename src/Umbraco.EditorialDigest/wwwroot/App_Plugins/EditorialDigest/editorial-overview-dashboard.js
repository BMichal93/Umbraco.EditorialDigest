import { umbHttpClient } from "@umbraco-cms/backoffice/http-client";

const overviewEndpoint = "/umbraco/management/api/v1/editorial-digest/dashboard/overview";

class EditorialOverviewDashboard extends HTMLElement {
    connectedCallback() {
        this.load();
    }

    async load() {
        this.render("Loading editorial overview...");

        try {
            const result = await umbHttpClient.get({ url: overviewEndpoint });
            if (result.error || !result.data) throw new Error("Unable to load the editorial overview.");

            this.renderOverview(result.data);
        } catch (error) {
            this.render(error.message);
        }
    }

    render(message) {
        this.innerHTML = `<uui-box headline="Editorial Overview"><p>${escapeHtml(message)}</p></uui-box>`;
    }

    renderOverview(data) {
        const sections = data.sections.map(section => `
            <uui-box headline="${sectionLabel(section.section)}">
                ${section.items.length === 0 ? "<p>No items to report.</p>" : `<ul>${section.items.map(item => `<li><strong>${escapeHtml(item.name)}</strong> <small>${escapeHtml(item.context)}</small></li>`).join("")}</ul>`}
            </uui-box>`).join("");
        const digests = data.activeDigests.length === 0
            ? "<p>No active digests.</p>"
            : `<table><thead><tr><th>Name</th><th>Recipients</th><th>Last run</th></tr></thead><tbody>${data.activeDigests.map(digest => `<tr><td>${escapeHtml(digest.name)}</td><td>${digest.recipientCount}</td><td>${formatDate(digest.lastRunDate)}</td></tr>`).join("")}</tbody></table>`;

        this.innerHTML = `
            <style>
                :host { display:block; max-width:1100px; margin:24px auto; }
                .actions { display:flex; justify-content:space-between; align-items:center; margin-bottom:16px; }
                .grid { display:grid; gap:16px; grid-template-columns:repeat(auto-fit,minmax(320px,1fr)); }
                table { border-collapse:collapse; width:100%; } th, td { padding:8px; text-align:left; border-bottom:1px solid #d8d7d9; } small { color:#667085; }
            </style>
            <div class="actions"><h1>Editorial Overview</h1><uui-button look="secondary" label="Refresh">Refresh</uui-button></div>
            <div class="grid">${sections}</div>
            <uui-box headline="Active Digests">${digests}</uui-box>`;
        this.querySelector("uui-button").addEventListener("click", () => this.load());
    }
}

function formatDate(value) {
    return value ? new Intl.DateTimeFormat(undefined, { dateStyle: "medium", timeStyle: "short" }).format(new Date(value)) : "Never";
}

function sectionLabel(value) {
    return ["Recently Published", "Upcoming Scheduled Content", "Stuck Workflows", "Pending Review", "Expiring Content", "Stale Content", "Broken Links"][value] ?? "Editorial items";
}

function escapeHtml(value) {
    return String(value ?? "").replace(/[&<>'"]/g, character => ({ "&":"&amp;", "<":"&lt;", ">":"&gt;", "'":"&#39;", "\"":"&quot;" })[character]);
}

customElements.define("editorial-overview-dashboard", EditorialOverviewDashboard);
