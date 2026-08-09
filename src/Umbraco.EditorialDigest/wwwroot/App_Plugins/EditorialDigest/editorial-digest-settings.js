import { umbHttpClient } from "@umbraco-cms/backoffice/http-client";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

const apiRoot = "/umbraco/management/api/v1/editorial-digest";
const defaultConfig = () => ({
    id: 0, name: "", alias: "", description: "", isEnabled: true, recipientSource: 0,
    recipientUserGroups: "", scheduleType: 0, scheduleDay: null, scheduleTime: "09:00:00", timeZoneId: browserTimeZone(),
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
            this.innerHTML = `<uui-box headline="Editorial Digest"><p role="alert">${escapeHtml(error.message)}</p></uui-box>`;
        }
    }

    render() {
        this.innerHTML = `
            <style>
                :host { display:block; max-width:1120px; margin:32px auto; color:var(--uui-color-text, #1d1d1f); }
                .intro, .toolbar, .digest-card__header, .setting-toggle, .form-actions { display:flex; align-items:center; gap:12px; }
                .intro { justify-content:space-between; margin-bottom:24px; }
                h1, h2, p { margin:0; } h1 { font-size:28px; line-height:1.2; } h2 { font-size:18px; }
                .eyebrow, .muted, .digest-card__meta, .field-help { color:var(--uui-color-text-alt, #6b6b6b); }
                .eyebrow { font-size:12px; font-weight:700; letter-spacing:.06em; text-transform:uppercase; margin-bottom:6px; }
                .intro p:last-child { margin-top:6px; max-width:700px; } uui-box { margin-bottom:16px; }
                .digest-list { display:grid; gap:10px; } .digest-card { border:1px solid var(--uui-color-border, #d9d9d9); border-radius:8px; padding:16px; }
                .digest-card__header { justify-content:space-between; } .digest-card__title { display:flex; align-items:center; flex-wrap:wrap; gap:8px; }
                .digest-card__meta { margin:8px 0 14px; font-size:14px; } .digest-card__actions { display:flex; flex-wrap:wrap; gap:8px; }
                .status { border-radius:999px; font-size:12px; font-weight:600; padding:3px 8px; background:#e8f5ec; color:#185c37; }
                .status--paused { background:#f2f2f2; color:#555; } .empty { text-align:center; padding:32px 16px; }
                form { display:grid; gap:20px; } .form-section { display:grid; gap:12px; }
                .form-section > p { margin-top:-6px; } label { display:grid; gap:6px; font-weight:600; }
                input, textarea, select { box-sizing:border-box; width:100%; padding:9px 10px; font:inherit; border:1px solid var(--uui-color-border, #b7b7b7); border-radius:4px; background:var(--uui-color-surface, #fff); }
                textarea { min-height:78px; resize:vertical; } input[type=checkbox] { width:auto; } input[type=time] { min-height:38px; }
                .two, .three { display:grid; grid-template-columns:repeat(2, minmax(0, 1fr)); gap:12px; } .three { grid-template-columns:repeat(3, minmax(0, 1fr)); }
                .toggle-label { display:flex; align-items:center; gap:8px; font-weight:600; }
                fieldset { border:0; padding:0; margin:0; } legend { font-weight:700; margin-bottom:10px; }
                .section-list { display:grid; grid-template-columns:repeat(2, minmax(0, 1fr)); gap:8px; }
                .section-option { display:flex; align-items:center; gap:8px; padding:10px; font-weight:500; border:1px solid var(--uui-color-border, #d9d9d9); border-radius:6px; }
                details { border-top:1px solid var(--uui-color-border, #d9d9d9); padding-top:16px; } summary { cursor:pointer; font-weight:700; }
                details > *:not(summary) { margin-top:16px; } .advanced-fields { display:grid; gap:12px; }
                .field-help { font-size:13px; font-weight:400; } .form-actions { justify-content:flex-end; border-top:1px solid var(--uui-color-border, #d9d9d9); padding-top:16px; }
                .setting-toggle { justify-content:space-between; padding:8px 0; } [hidden] { display:none !important; }
                @media (max-width:720px) { :host { margin:20px 12px; } .intro { align-items:flex-start; flex-direction:column; } .two, .three, .section-list { grid-template-columns:1fr; } .form-actions { justify-content:flex-start; flex-wrap:wrap; } }
            </style>
            <section class="intro">
                <div><p class="eyebrow">Content operations</p><h1>Editorial Digest</h1><p class="muted">A simple, scheduled view of what your content team needs to know.</p></div>
                <uui-button id="new-config" look="primary" label="Create digest">Create digest</uui-button>
            </section>
            <uui-box headline="Your digests">${digestList(this.configs)}</uui-box>
            <uui-box headline="Package controls">${globalSettingsForm(this.settings)}</uui-box>
            ${this.editor ? `<uui-box headline="${this.editor.id ? "Edit digest" : "Create digest"}">${configForm(this.editor)}</uui-box>` : ""}`;

        this.bindEvents();
    }

    bindEvents() {
        this.querySelector("#new-config")?.addEventListener("click", () => {
            this.editor = defaultConfig();
            this.render();
        });
        this.querySelectorAll("[data-edit]").forEach(button => button.addEventListener("click", () => this.load(Number(button.dataset.edit))));
        this.querySelectorAll("[data-duplicate]").forEach(button => button.addEventListener("click", () => this.duplicate(Number(button.dataset.duplicate))));
        this.querySelectorAll("[data-delete]").forEach(button => button.addEventListener("click", () => this.remove(Number(button.dataset.delete))));
        this.querySelector("#global-settings")?.addEventListener("submit", event => this.saveGlobal(event));
        this.querySelector("#digest-config")?.addEventListener("submit", event => this.saveConfig(event));
        this.querySelector("#cancel-editor")?.addEventListener("click", () => { this.editor = null; this.render(); });
        this.querySelector("#run-now")?.addEventListener("click", () => this.runNow());
        this.querySelector("#preview")?.addEventListener("click", () => window.open(`${apiRoot}/configurations/${this.editor.id}/delivery/preview`, "_blank", "noopener"));
        this.bindFormAssistance();
    }

    bindFormAssistance() {
        const form = this.querySelector("#digest-config");
        if (!form) return;

        const name = form.elements.namedItem("name");
        const alias = form.elements.namedItem("alias");
        name.addEventListener("input", () => {
            if (!this.editor.id && alias.dataset.edited !== "true") alias.value = slugify(name.value);
        });
        alias.addEventListener("input", () => { alias.dataset.edited = "true"; });

        const scheduleType = form.elements.namedItem("scheduleType");
        const scheduleDay = this.querySelector("[data-schedule-day]");
        const updateSchedule = () => { scheduleDay.hidden = scheduleType.value !== "1"; };
        scheduleType.addEventListener("change", updateSchedule);
        updateSchedule();

        const recipientSource = form.elements.namedItem("recipientSource");
        const groups = this.querySelector("[data-recipient-groups]");
        const updateRecipients = () => { groups.hidden = recipientSource.value === "1"; };
        recipientSource.addEventListener("change", updateRecipients);
        updateRecipients();
    }

    async request(path, options = {}) {
        return request(path, options);
    }

    async saveGlobal(event) {
        event.preventDefault();
        const form = new FormData(event.currentTarget);
        await this.execute(() => this.request("/settings", { method: "PUT", body: JSON.stringify({
            defaultFromName: form.get("defaultFromName"), defaultFromEmail: form.get("defaultFromEmail") || null,
            logoUrl: form.get("logoUrl") || null, customTemplateBasePath: form.get("customTemplateBasePath") || null,
            dashboardRefreshMinutes: Number(form.get("dashboardRefreshMinutes")), isPackageEnabled: form.get("isPackageEnabled") === "on",
            loggingLevel: Number(form.get("loggingLevel"))
        }) }).then(() => this.load()));
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

function digestList(configs) {
    if (!configs.length) return `<div class="empty"><h2>Create your first digest</h2><p class="muted">Start with a daily summary and adjust the details only when you need to.</p></div>`;

    return `<div class="digest-list">${configs.map(config => `<article class="digest-card">
        <div class="digest-card__header"><div class="digest-card__title"><h2>${escapeHtml(config.name)}</h2><span class="status${config.isEnabled ? "" : " status--paused"}">${config.isEnabled ? "Active" : "Paused"}</span></div></div>
        <p class="digest-card__meta">${escapeHtml(scheduleSummary(config))} · ${escapeHtml(recipientSummary(config))} · Last run: ${formatDate(config.lastRunDate)}</p>
        <div class="digest-card__actions"><uui-button data-edit="${config.id}" look="primary" label="Edit ${escapeHtml(config.name)}">Edit</uui-button><uui-button data-duplicate="${config.id}" look="secondary" label="Duplicate ${escapeHtml(config.name)}">Duplicate</uui-button><uui-button data-delete="${config.id}" look="danger" label="Delete ${escapeHtml(config.name)}">Delete</uui-button></div>
    </article>`).join("")}</div>`;
}

function globalSettingsForm(settings) { return `<form id="global-settings">
    <div class="setting-toggle"><div><h2>${settings.isPackageEnabled ? "Digests are sending" : "Digests are paused"}</h2><p class="muted">Turn this off to pause every digest without changing its setup.</p></div><label class="toggle-label"><input name="isPackageEnabled" type="checkbox" ${settings.isPackageEnabled ? "checked" : ""}> Enabled</label></div>
    <details><summary>Sender defaults and advanced settings</summary><div class="advanced-fields">
        <div class="two"><label>Default sender name<input name="defaultFromName" value="${escapeHtml(settings.defaultFromName)}"></label><label>Default sender email<input name="defaultFromEmail" type="email" value="${escapeHtml(settings.defaultFromEmail)}"></label></div>
        <div class="two"><label>Logo URL<input name="logoUrl" type="url" value="${escapeHtml(settings.logoUrl)}"></label><label>Template base path<input name="customTemplateBasePath" value="${escapeHtml(settings.customTemplateBasePath)}"></label></div>
        <div class="two"><label>Dashboard refresh (minutes)<input name="dashboardRefreshMinutes" type="number" min="1" max="60" value="${settings.dashboardRefreshMinutes}"></label><label>Logging level<select name="loggingLevel"><option value="0" ${selected(settings.loggingLevel, 0)}>Minimal</option><option value="1" ${selected(settings.loggingLevel, 1)}>Normal</option><option value="2" ${selected(settings.loggingLevel, 2)}>Verbose</option></select></label></div>
    </div></details>
    <div class="form-actions"><uui-button type="submit" look="primary" label="Save package controls">Save package controls</uui-button></div>
</form>`; }

function configForm(config) { return `<form id="digest-config">
    <section class="form-section"><h2>1. Start with the essentials</h2><p class="muted">Give the digest a clear name and decide whether it is ready to send.</p>
        <label>Name<input name="name" required maxlength="255" autocomplete="off" value="${escapeHtml(config.name)}"></label>
        <label>Description <span class="field-help">Optional. Helps other administrators understand the purpose.</span><textarea name="description" maxlength="1000">${escapeHtml(config.description)}</textarea></label>
        <label class="toggle-label"><input name="isEnabled" type="checkbox" ${config.isEnabled ? "checked" : ""}> This digest is active</label>
    </section>
    <section class="form-section"><h2>2. Choose who receives it</h2>
        <label>Recipients<select name="recipientSource"><option value="0" ${selected(config.recipientSource, 0)}>Umbraco user groups</option><option value="1" ${selected(config.recipientSource, 1)}>Custom mailing list</option><option value="2" ${selected(config.recipientSource, 2)}>User groups and custom mailing list</option></select></label>
        <label data-recipient-groups>User group aliases <span class="field-help">Separate aliases with commas.</span><input name="recipientUserGroups" value="${escapeHtml(config.recipientUserGroups)}" placeholder="editors, marketing"></label>
    </section>
    <section class="form-section"><h2>3. Pick a schedule</h2><div class="three">
        <label>Frequency<select name="scheduleType"><option value="0" ${selected(config.scheduleType, 0)}>Daily</option><option value="1" ${selected(config.scheduleType, 1)}>Weekly</option></select></label>
        <label data-schedule-day>Day<select name="scheduleDay">${weekdayOptions(config.scheduleDay ?? 1)}</select></label>
        <label>Time<input name="scheduleTime" type="time" step="60" required value="${escapeHtml(config.scheduleTime)}"></label>
    </div><label>Time zone<input name="timeZoneId" required value="${escapeHtml(config.timeZoneId)}" placeholder="Europe/Copenhagen"></label></section>
    <section class="form-section"><h2>4. Select what to include</h2><fieldset><legend class="field-help">Choose only the information this team should see.</legend><div class="section-list">${sectionCheckboxes(config.sectionsEnabled)}</div></fieldset></section>
    <details><summary>Advanced options</summary><div class="advanced-fields">
        <label>Alias <span class="field-help">Created automatically from the name. Use lowercase letters, numbers, and hyphens.</span><input name="alias" required pattern="[a-z0-9-]+" value="${escapeHtml(config.alias)}"></label>
        <div class="three"><label>Published lookback (hours)<input name="lookbackHours" type="number" min="1" max="720" value="${config.lookbackHours}"></label><label>Upcoming window (hours)<input name="upcomingHours" type="number" min="1" max="720" value="${config.upcomingHours}"></label><label>Stale after (days)<input name="staleDays" type="number" min="1" max="3650" value="${config.staleDays}"></label></div>
        <div class="two"><label>Expiring window (days)<input name="expiringDays" type="number" min="1" max="365" value="${config.expiringDays}"></label><label>Maximum items per section<input name="maxItemsPerSection" type="number" min="1" max="50" value="${config.maxItemsPerSection}"></label></div>
        <label>Subject line template<input name="subjectLineTemplate" required value="${escapeHtml(config.subjectLineTemplate)}"></label>
        <div class="three"><label>From name<input name="fromName" value="${escapeHtml(config.fromName)}"></label><label>From email<input name="fromEmail" type="email" value="${escapeHtml(config.fromEmail)}"></label><label>Reply-to email<input name="replyToEmail" type="email" value="${escapeHtml(config.replyToEmail)}"></label></div>
        <label>Custom template path<input name="customTemplatePath" value="${escapeHtml(config.customTemplatePath)}"></label>
    </div></details>
    <div class="form-actions"><uui-button id="cancel-editor" type="button" look="secondary" label="Cancel">Cancel</uui-button>${config.id ? '<uui-button id="preview" type="button" look="secondary" label="Preview digest">Preview</uui-button><uui-button id="run-now" type="button" look="secondary" label="Run digest now">Run now</uui-button>' : ""}<uui-button type="submit" look="primary" label="Save digest">Save digest</uui-button></div>
</form>`; }

function sectionCheckboxes(enabled) { return [[0, "Recently published"], [1, "Upcoming scheduled content"], [2, "Stuck workflows"], [3, "Pending review"], [4, "Expiring content"], [5, "Stale content"], [6, "Broken links"]].map(([value, label]) => `<label class="section-option"><input name="sectionsEnabled" type="checkbox" value="${value}" ${enabled.includes(value) ? "checked" : ""}> ${label}</label>`).join(""); }

async function request(path, options = {}) {
    const method = (options.method || "GET").toLowerCase();
    const result = await umbHttpClient[method]({
        url: `${apiRoot}${path}`,
        body: options.body ? JSON.parse(options.body) : undefined,
        headers: options.headers,
        security: [{ type: "http", scheme: "bearer" }]
    });

    if (result.error) throw new Error(result.error.detail || result.error.title || "The request could not be completed.");
    return result.data ?? null;
}

function scheduleSummary(config) { return `${config.scheduleType === 1 ? `Every ${weekdayName(config.scheduleDay)}` : "Daily"} at ${formatTime(config.scheduleTime)} (${config.timeZoneId || "UTC"})`; }
function recipientSummary(config) { return ["User groups", "Mailing list", "User groups and mailing list"][config.recipientSource] || "Recipients not set"; }
function weekdayOptions(selectedDay) { return ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"].map((name, value) => `<option value="${value}" ${selected(selectedDay, value)}>${name}</option>`).join(""); }
function weekdayName(value) { return ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"][Number(value)] || "Monday"; }
function browserTimeZone() { return Intl.DateTimeFormat().resolvedOptions().timeZone || "UTC"; }
function slugify(value) { return value.toLowerCase().normalize("NFD").replace(/[\u0300-\u036f]/g, "").replace(/[^a-z0-9]+/g, "-").replace(/^-+|-+$/g, ""); }
function selected(value, option) { return Number(value) === option ? "selected" : ""; }
function formatDate(value) { return value ? new Intl.DateTimeFormat(undefined, { dateStyle: "medium", timeStyle: "short" }).format(new Date(value)) : "Never"; }
function formatTime(value) { return String(value || "").slice(0, 5) || "09:00"; }
function escapeHtml(value) { return String(value ?? "").replace(/[&<>'"]/g, character => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", "'": "&#39;", "\"": "&quot;" })[character]); }

customElements.define("editorial-digest-settings", EditorialDigestSettings);
