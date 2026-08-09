import { umbHttpClient } from "@umbraco-cms/backoffice/http-client";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

const apiRoot = "/umbraco/management/api/v1/editorial-digest";
const defaultConfig = () => ({
    id: 0, name: "", alias: "", description: "", isEnabled: true, recipientSource: 0,
    recipientUserGroups: "", scheduleType: 0, scheduleDay: null, scheduleTime: "09:00:00", timeZoneId: "UTC",
    sectionsEnabled: [0, 1], lookbackHours: 24, upcomingHours: 48, staleDays: 90, expiringDays: 7,
    maxItemsPerSection: 10, subjectLineTemplate: "{{digestName}} — Editorial Digest for {{date}}",
    fromName: "", fromEmail: "", replyToEmail: "", customTemplatePath: ""
});

class EditorialDigestSettings extends UmbElementMixin(HTMLElement) {
    async connectedCallback() {
        await this.load();
    }

    async load(selectedId) {
        try {
            const [configs, settings] = await Promise.all([
                this.request("/configurations"),
                this.request("/settings")
            ]);
            this.configs = configs;
            this.settings = settings;
            this.editor = selectedId ? await this.request(`/configurations/${selectedId}`) : this.editor;
            this.render();
        } catch (error) {
            this.innerHTML = `<uui-box headline="Editorial Digest"><p>${escapeHtml(error.message)}</p></uui-box>`;
        }
    }

    render() {
        const configRows = this.configs.map(config => `<tr>
            <td>${escapeHtml(config.name)}</td><td>${config.isEnabled ? "Enabled" : "Disabled"}</td>
            <td>${formatDate(config.lastRunDate)}</td>
            <td><uui-button data-edit="${config.id}" look="secondary" label="Edit">Edit</uui-button>
            <uui-button data-duplicate="${config.id}" look="secondary" label="Duplicate">Duplicate</uui-button>
            <uui-button data-delete="${config.id}" look="danger" label="Delete">Delete</uui-button></td>
        </tr>`).join("");

        this.innerHTML = `
            <style>
                :host { display:block; max-width:1200px; margin:24px auto; } .toolbar { display:flex; gap:8px; align-items:center; margin-bottom:16px; }
                .layout { display:grid; grid-template-columns:minmax(0, 2fr) minmax(280px, 1fr); gap:16px; } table { width:100%; border-collapse:collapse; }
                th, td { padding:8px; border-bottom:1px solid #d8d7d9; text-align:left; } form { display:grid; gap:12px; } label { display:grid; gap:4px; font-weight:600; }
                input, textarea, select { box-sizing:border-box; width:100%; padding:8px; } .two { display:grid; grid-template-columns:repeat(2,minmax(0,1fr)); gap:12px; }
                .checklist { display:grid; grid-template-columns:repeat(2,minmax(0,1fr)); gap:6px; } .checklist label { display:flex; align-items:center; gap:6px; font-weight:400; }
                @media (max-width:800px) { .layout { grid-template-columns:1fr; } .two { grid-template-columns:1fr; } }
            </style>
            <div class="toolbar"><h1>Editorial Digest</h1><uui-button id="new-config" look="primary" label="Create digest">Create digest</uui-button></div>
            <div class="layout">
                <uui-box headline="Digest configurations"><table><thead><tr><th>Name</th><th>Status</th><th>Last run</th><th>Actions</th></tr></thead><tbody>${configRows || "<tr><td colspan=\"4\">No digests yet.</td></tr>"}</tbody></table></uui-box>
                <uui-box headline="Global settings">${globalSettingsForm(this.settings)}</uui-box>
            </div>
            ${this.editor ? `<uui-box headline="${this.editor.id ? "Edit digest" : "Create digest"}">${configForm(this.editor)}</uui-box>` : ""}`;

        this.bindEvents();
    }

    bindEvents() {
        this.querySelector("#new-config").addEventListener("click", () => { this.editor = defaultConfig(); this.render(); });
        this.querySelectorAll("[data-edit]").forEach(button => button.addEventListener("click", () => this.load(Number(button.dataset.edit))));
        this.querySelectorAll("[data-duplicate]").forEach(button => button.addEventListener("click", () => this.duplicate(Number(button.dataset.duplicate))));
        this.querySelectorAll("[data-delete]").forEach(button => button.addEventListener("click", () => this.remove(Number(button.dataset.delete))));
        this.querySelector("#global-settings")?.addEventListener("submit", event => this.saveGlobal(event));
        this.querySelector("#digest-config")?.addEventListener("submit", event => this.saveConfig(event));
        this.querySelector("#run-now")?.addEventListener("click", () => this.runNow());
        this.querySelector("#preview")?.addEventListener("click", () => window.open(`${apiRoot}/configurations/${this.editor.id}/delivery/preview`, "_blank", "noopener"));
    }

    async request(path, options = {}) {
        return request(this, path, options);
    }

    async saveGlobal(event) {
        event.preventDefault();
        const form = new FormData(event.currentTarget);
        await this.execute(() => this.request("/settings", { method: "PUT", body: JSON.stringify({
            defaultFromName: form.get("defaultFromName"), defaultFromEmail: form.get("defaultFromEmail") || null,
            logoUrl: form.get("logoUrl") || null, customTemplateBasePath: form.get("customTemplateBasePath") || null,
            dashboardRefreshMinutes: Number(form.get("dashboardRefreshMinutes")), isPackageEnabled: form.get("isPackageEnabled") === "on",
            loggingLevel: Number(form.get("loggingLevel"))
        }) }));
    }

    async saveConfig(event) {
        event.preventDefault();
        const form = new FormData(event.currentTarget);
        const sectionsEnabled = [...event.currentTarget.querySelectorAll("[name=sectionsEnabled]:checked")].map(input => Number(input.value));
        const payload = {
            ...defaultConfig(),
            name: form.get("name"), alias: form.get("alias"), description: form.get("description") || null,
            isEnabled: form.get("isEnabled") === "on", recipientSource: Number(form.get("recipientSource")),
            recipientUserGroups: form.get("recipientUserGroups") || null, scheduleType: Number(form.get("scheduleType")),
            scheduleDay: form.get("scheduleType") === "1" ? Number(form.get("scheduleDay")) : null, scheduleTime: form.get("scheduleTime"),
            timeZoneId: form.get("timeZoneId"), sectionsEnabled, lookbackHours: Number(form.get("lookbackHours")),
            upcomingHours: Number(form.get("upcomingHours")), staleDays: Number(form.get("staleDays")), expiringDays: Number(form.get("expiringDays")),
            maxItemsPerSection: Number(form.get("maxItemsPerSection")), subjectLineTemplate: form.get("subjectLineTemplate"),
            fromName: form.get("fromName") || null, fromEmail: form.get("fromEmail") || null, replyToEmail: form.get("replyToEmail") || null,
            customTemplatePath: form.get("customTemplatePath") || null
        };
        const id = this.editor.id;
        await this.execute(async () => {
            this.editor = await this.request(`/configurations${id ? `/${id}` : ""}`, { method: id ? "PUT" : "POST", body: JSON.stringify(payload) });
            await this.load(this.editor.id);
        });
    }

    async duplicate(id) { await this.execute(() => this.request(`/configurations/${id}/duplicate`, { method: "POST" }).then(config => this.load(config.id))); }
    async remove(id) { if (confirm("Delete this digest configuration?")) await this.execute(() => this.request(`/configurations/${id}`, { method: "DELETE" }).then(() => { this.editor = null; return this.load(); })); }
    async runNow() { if (confirm("Send this digest to all active recipients now?")) await this.execute(() => this.request(`/configurations/${this.editor.id}/delivery/run`, { method: "POST" }).then(() => this.load(this.editor.id))); }
    async execute(action) { try { await action(); } catch (error) { alert(error.message); } }
}

function globalSettingsForm(settings) { return `<form id="global-settings">
    <label>Default sender name<input name="defaultFromName" value="${escapeHtml(settings.defaultFromName)}"></label>
    <label>Default sender email<input name="defaultFromEmail" type="email" value="${escapeHtml(settings.defaultFromEmail)}"></label>
    <label>Logo URL<input name="logoUrl" type="url" value="${escapeHtml(settings.logoUrl)}"></label>
    <label>Template base path<input name="customTemplateBasePath" value="${escapeHtml(settings.customTemplateBasePath)}"></label>
    <label>Dashboard refresh minutes<input name="dashboardRefreshMinutes" type="number" min="1" max="60" value="${settings.dashboardRefreshMinutes}"></label>
    <label>Logging level<select name="loggingLevel"><option value="0" ${selected(settings.loggingLevel, 0)}>Minimal</option><option value="1" ${selected(settings.loggingLevel, 1)}>Normal</option><option value="2" ${selected(settings.loggingLevel, 2)}>Verbose</option></select></label>
    <label><input name="isPackageEnabled" type="checkbox" ${settings.isPackageEnabled ? "checked" : ""}> Enable package</label><uui-button type="submit" look="primary" label="Save global settings">Save global settings</uui-button></form>`; }

function configForm(config) { return `<form id="digest-config">
    <div class="two"><label>Name<input name="name" required value="${escapeHtml(config.name)}"></label><label>Alias<input name="alias" required pattern="[a-z0-9-]+" value="${escapeHtml(config.alias)}"></label></div>
    <label>Description<textarea name="description">${escapeHtml(config.description)}</textarea></label><label><input name="isEnabled" type="checkbox" ${config.isEnabled ? "checked" : ""}> Enable this digest</label>
    <div class="two"><label>Recipient source<select name="recipientSource"><option value="0" ${selected(config.recipientSource,0)}>Umbraco user groups</option><option value="1" ${selected(config.recipientSource,1)}>Custom mailing list</option><option value="2" ${selected(config.recipientSource,2)}>Both</option></select></label><label>User group aliases (comma-separated)<input name="recipientUserGroups" value="${escapeHtml(config.recipientUserGroups)}"></label></div>
    <div class="two"><label>Schedule<select name="scheduleType"><option value="0" ${selected(config.scheduleType,0)}>Daily</option><option value="1" ${selected(config.scheduleType,1)}>Weekly</option></select></label><label>Week day (0 Sunday - 6 Saturday)<input name="scheduleDay" type="number" min="0" max="6" value="${config.scheduleDay ?? 1}"></label><label>Time<input name="scheduleTime" value="${escapeHtml(config.scheduleTime)}" placeholder="09:00:00"></label><label>Time zone<input name="timeZoneId" required value="${escapeHtml(config.timeZoneId)}"></label></div>
    <fieldset><legend>Digest sections</legend><div class="checklist">${sectionCheckboxes(config.sectionsEnabled)}</div></fieldset>
    <div class="two"><label>Published lookback hours<input name="lookbackHours" type="number" min="1" max="720" value="${config.lookbackHours}"></label><label>Upcoming hours<input name="upcomingHours" type="number" min="1" max="720" value="${config.upcomingHours}"></label><label>Stale days<input name="staleDays" type="number" min="1" max="3650" value="${config.staleDays}"></label><label>Expiring days<input name="expiringDays" type="number" min="1" max="365" value="${config.expiringDays}"></label><label>Maximum items per section<input name="maxItemsPerSection" type="number" min="1" max="50" value="${config.maxItemsPerSection}"></label></div>
    <label>Subject line template<input name="subjectLineTemplate" required value="${escapeHtml(config.subjectLineTemplate)}"></label><div class="two"><label>From name<input name="fromName" value="${escapeHtml(config.fromName)}"></label><label>From email<input name="fromEmail" type="email" value="${escapeHtml(config.fromEmail)}"></label><label>Reply-to email<input name="replyToEmail" type="email" value="${escapeHtml(config.replyToEmail)}"></label><label>Custom template path<input name="customTemplatePath" value="${escapeHtml(config.customTemplatePath)}"></label></div>
    <div class="toolbar"><uui-button type="submit" look="primary" label="Save digest">Save digest</uui-button>${config.id ? '<uui-button id="preview" type="button" look="secondary" label="Preview">Preview</uui-button><uui-button id="run-now" type="button" look="secondary" label="Run now">Run now</uui-button>' : ""}</div></form>`; }

function sectionCheckboxes(enabled) { return [[0,"Recently published"],[1,"Upcoming scheduled content"],[2,"Stuck workflows"],[3,"Pending review"],[4,"Expiring content"],[5,"Stale content"],[6,"Broken links"]].map(([value, label]) => `<label><input name="sectionsEnabled" type="checkbox" value="${value}" ${enabled.includes(value) ? "checked" : ""}> ${label}</label>`).join(""); }
async function request(host, path, options = {}) {
    const method = (options.method || "GET").toLowerCase();
    const requestMethod = umbHttpClient[method];
    const result = await requestMethod({
        url: `${apiRoot}${path}`,
        body: options.body ? JSON.parse(options.body) : undefined,
        headers: options.headers,
        security: [{ type: "http", scheme: "bearer" }]
    });

    if (result.error) throw new Error(result.error.detail || result.error.title || "The request could not be completed.");
    return result.data ?? null;
}
function selected(value, option) { return Number(value) === option ? "selected" : ""; }
function formatDate(value) { return value ? new Intl.DateTimeFormat(undefined, { dateStyle:"medium", timeStyle:"short" }).format(new Date(value)) : "Never"; }
function escapeHtml(value) { return String(value ?? "").replace(/[&<>'"]/g, character => ({ "&":"&amp;", "<":"&lt;", ">":"&gt;", "'":"&#39;", "\"":"&quot;" })[character]); }

customElements.define("editorial-digest-settings", EditorialDigestSettings);
