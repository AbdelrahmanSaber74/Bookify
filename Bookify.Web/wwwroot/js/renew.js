$(document).ready(function () {
    $('.js-renew').on('click', function () {
        const subscriberKey = $(this).data('key');

        bootbox.confirm({
            message: "Are you sure that you need to renew this subscription?",
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-success'
                },
                cancel: {
                    label: 'No',
                    className: 'btn-secondary'
                }
            },
            callback: function (result) {
                if (result) {
                    $.post({
                        url: `/Subscribers/RenewSubscription?sKey=${subscriberKey}`,
                        data: {
                            '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                        },
                        success: function (row) {
                            const $subscriptionsTable = $('#SubscriptionsTable').find('tbody');
                            const $activeIcon = $('#ActiveStatusIcon');
                            const $card = $activeIcon.parents('.card');
                            const $statusBadge = $('#StatusBadge');
                            var rentalButton = document.getElementById("RentalButton");

                            // Append the new row to the subscriptions table
                            $subscriptionsTable.append(row);

                            // Update icon and status visuals
                            $activeIcon.removeClass('d-none').siblings('svg').remove();
                            rentalButton.classList.remove("d-none");
                            $card.removeClass('bg-warning').addClass('bg-success');
                            $('#CardStatus').text('Active subscriber');
                            $statusBadge.removeClass('badge-light-warning').addClass('badge-light-success').text('Active subscriber');

                            // Display success message
                            showSuccessMessage();
                        },
                        error: function () {
                            showErrorMessage();
                        }
                    });
                }
            }
        });
    });
});
