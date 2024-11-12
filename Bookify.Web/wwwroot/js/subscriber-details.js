$(document).ready(function () {

    // Renew Subscription
    $('.js-renew').on('click', function () {
        const subscriberKey = $(this).data('key');

        bootbox.confirm({
            message: "Are you sure you want to renew this subscription?",
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
                            updateSubscriptionTable(row);
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
    // Cancel Rental
    // Cancel Rental
    $('.js-cancel-rental').on('click', function () {
        const rentalId = $(this).data('id');

        bootbox.confirm({
            message: "Are you sure you want to cancel this rental?",
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-danger'
                },
                cancel: {
                    label: 'No',
                    className: 'btn-secondary'
                }
            },
            callback: function (confirmed) {
                if (confirmed) {
                    $.ajax({
                        url: '/Rentals/MarkAsDeleted',
                        type: 'POST',
                        data: {
                            id: rentalId,
                            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                        },
                        success: function (response) {
                            // Remove the rental row from the table
                            removeRentalRow(rentalId);

                            // Check if there are remaining rentals and adjust the table display
                            if ($("#RentalsTable tbody tr").length === 0) {
                                $("#RentalsTable").fadeOut();
                                $("#Alert").fadeIn();
                            }

                            // Update the rental copies count display
                            const totalCountElement = $("#rentalCopiesCount");
                            const currentCount = parseInt(totalCountElement.text(), 10);
                            totalCountElement.text(currentCount - response.count);

                            // Show a success message
                            showSuccessMessage("Rental deleted successfully.");
                        },
                        error: function () {
                            showErrorMessage("Error occurred while deleting the rental.");
                        }
                    });
                }
            }
        });
    });


    // Update Subscription Table and UI
    function updateSubscriptionTable(row) {
        const $subscriptionsTable = $('#SubscriptionsTable').find('tbody');
        const $activeIcon = $('#ActiveStatusIcon');
        const $card = $activeIcon.parents('.card');
        const $statusBadge = $('#StatusBadge');
        const rentalButton = document.getElementById("RentalButton");

        // Append new row to the subscriptions table
        $subscriptionsTable.append(row);

        // Update icon and status visuals
        $activeIcon.removeClass('d-none').siblings('svg').remove();
        rentalButton.classList.remove("d-none");
        $card.removeClass('bg-warning').addClass('bg-success');
        $('#CardStatus').text('Active subscriber');
        $statusBadge.removeClass('badge-light-warning').addClass('badge-light-success').text('Active subscriber');
    }

    // Remove Rental Row from the Table
    function removeRentalRow(rentalId) {
        $(`tr[data-id="${rentalId}"]`).remove();
    }




});
