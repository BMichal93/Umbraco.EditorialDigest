(function () {
    "use strict";

    angular.module("umbraco").config(function ($routeProvider) {
        $routeProvider.when("/settings/editorialDigest/digests", {
            templateUrl: "/App_Plugins/EditorialDigest/digests.html",
            controller: "EditorialDigest.DigestsController",
            controllerAs: "vm"
        });
    });

    angular.module("umbraco").controller("EditorialDigest.DigestsController", function ($http, $window, notificationsService) {
        var vm = this;
        var endpoint = "/umbraco/backoffice/EditorialDigest/DigestConfigApi/";

        vm.configs = [];
        vm.editor = null;
        vm.timeZones = [];
        vm.isSaving = false;
        vm.recipientSources = [
            { value: 0, label: "Umbraco User Groups" },
            { value: 1, label: "Custom Mailing List" },
            { value: 2, label: "Both" }
        ];
        vm.scheduleTypes = [
            { value: 0, label: "Daily" },
            { value: 1, label: "Weekly" }
        ];
        vm.weekdays = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
        vm.sections = [
            { value: 0, label: "Recently Published" },
            { value: 1, label: "Upcoming Scheduled Content" },
            { value: 2, label: "Stuck Workflows" },
            { value: 3, label: "Pages Pending Review" },
            { value: 4, label: "Expiring Content" },
            { value: 5, label: "Content Without Updates" },
            { value: 6, label: "Broken Links Found" }
        ];

        vm.newConfig = function () {
            vm.editor = {
                id: 0,
                name: "",
                alias: "",
                isEnabled: true,
                recipientSource: 0,
                scheduleType: 0,
                scheduleTime: "09:00:00",
                timeZoneId: "UTC",
                sectionsEnabled: [0, 1],
                lookbackHours: 24,
                upcomingHours: 48,
                staleDays: 90,
                expiringDays: 7,
                maxItemsPerSection: 10,
                subjectLineTemplate: "{{digestName}} — Editorial Digest for {{date}}"
            };
        };

        vm.edit = function (id) {
            $http.get(endpoint + "Get", { params: { id: id } }).then(function (response) {
                vm.editor = response.data;
            }).catch(showError);
        };

        vm.cancel = function () {
            vm.editor = null;
        };

        vm.save = function () {
            vm.isSaving = true;
            var action = vm.editor.id ? "Save?id=" + vm.editor.id : "Create";
            $http.post(endpoint + action, vm.editor).then(function (response) {
                notificationsService.success("Editorial Digest", "Digest configuration saved.");
                vm.editor = response.data;
                loadConfigs();
            }).catch(showError).finally(function () {
                vm.isSaving = false;
            });
        };

        vm.duplicate = function (id) {
            $http.post(endpoint + "Duplicate?id=" + id).then(function (response) {
                notificationsService.success("Editorial Digest", "Digest configuration duplicated as disabled.");
                vm.editor = response.data;
                loadConfigs();
            }).catch(showError);
        };

        vm.remove = function (config) {
            if (!$window.confirm("Delete the digest configuration '" + config.name + "'?")) {
                return;
            }

            $http.delete(endpoint + "Delete", { params: { id: config.id } }).then(function () {
                notificationsService.success("Editorial Digest", "Digest configuration deleted.");
                if (vm.editor && vm.editor.id === config.id) {
                    vm.editor = null;
                }
                loadConfigs();
            }).catch(showError);
        };

        vm.isSectionEnabled = function (section) {
            return vm.editor.sectionsEnabled.indexOf(section.value) !== -1;
        };

        vm.toggleSection = function (section) {
            var index = vm.editor.sectionsEnabled.indexOf(section.value);
            if (index === -1) {
                vm.editor.sectionsEnabled.push(section.value);
            } else {
                vm.editor.sectionsEnabled.splice(index, 1);
            }
        };

        function loadConfigs() {
            $http.get(endpoint + "GetAll").then(function (response) {
                vm.configs = response.data;
            }).catch(showError);
        }

        function showError(response) {
            var message = response.data && response.data.title ? response.data.title : "Unable to complete the request.";
            notificationsService.error("Editorial Digest", message);
        }

        $http.get(endpoint + "GetTimeZones").then(function (response) {
            vm.timeZones = response.data;
        }).catch(showError);

        loadConfigs();
    });
}());
