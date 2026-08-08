(function () {
    "use strict";

    function EditorialOverviewController($http) {
        var vm = this;
        vm.loading = true;

        vm.refresh = function () {
            vm.loading = true;
            $http.get("/umbraco/backoffice/EditorialDigest/DashboardApi/GetOverview")
                .then(function (response) { vm.overview = response.data; })
                .finally(function () { vm.loading = false; });
        };

        vm.refresh();
    }

    angular.module("umbraco").controller("EditorialDigest.EditorialOverviewController", EditorialOverviewController);
}());
