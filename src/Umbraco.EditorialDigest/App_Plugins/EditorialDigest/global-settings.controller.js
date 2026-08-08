(function () {
    "use strict";

    angular.module("umbraco").config(function ($routeProvider) {
        $routeProvider.when("/settings/editorialDigest/global-settings", {
            templateUrl: "/App_Plugins/EditorialDigest/global-settings.html",
            controller: "EditorialDigest.GlobalSettingsController",
            controllerAs: "vm"
        });

        $routeProvider.when("/settings/editorialDigest/digests", {
            templateUrl: "/App_Plugins/EditorialDigest/digests.html"
        });
    });

    angular.module("umbraco").controller("EditorialDigest.GlobalSettingsController", function ($http, notificationsService) {
        var vm = this;
        var endpoint = "/umbraco/backoffice/EditorialDigest/GlobalSettingsApi/";

        vm.settings = null;
        vm.isSaving = false;
        vm.loggingLevels = [
            { value: 0, label: "Minimal" },
            { value: 1, label: "Normal" },
            { value: 2, label: "Verbose" }
        ];

        vm.save = function () {
            vm.isSaving = true;
            $http.post(endpoint + "Save", vm.settings)
                .then(function () {
                    notificationsService.success("Editorial Digest", "Global settings saved.");
                })
                .catch(function (response) {
                    var message = response.data && response.data.title ? response.data.title : "Unable to save global settings.";
                    notificationsService.error("Editorial Digest", message);
                })
                .finally(function () {
                    vm.isSaving = false;
                });
        };

        $http.get(endpoint + "GetSettings")
            .then(function (response) {
                vm.settings = response.data;
            })
            .catch(function () {
                notificationsService.error("Editorial Digest", "Unable to load global settings.");
            });
    });
}());
