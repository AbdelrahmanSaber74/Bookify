var updatedRow;

// Show success message function
function showSuccessMessage(message = 'Saved successfully!') {
    Swal.fire({
        icon: 'success',
        title: 'Success',
        text: message,
        customClass: {
            confirmButton: "btn btn-primary"
        }
    });
}

// Show error message function
function showErrorMessage(message = 'Something went wrong!') {
    Swal.fire({
        icon: 'error',
        title: 'Oops...',
        text: message,
        customClass: {
            confirmButton: "btn btn-primary"
        }
    });
}

// Show warning message function
function showWarningMessage(message = 'Something went wrong!') {
    Swal.fire({
        icon: 'warning',
        title: 'Warning!',
        text: message,
        customClass: {
            confirmButton: "btn btn-warning"
        }
    });
}


// Document ready function to handle displaying success, error, or warning messages
$(document).ready(function () {
    var successMessage = $("#SuccessMessage").text().trim();
    if (successMessage) {
        showSuccessMessage(successMessage);
    }

    var errorMessage = $("#ErrorMessage").text().trim();
    if (errorMessage) {
        showErrorMessage(errorMessage);
    }

    var warningMessage = $("#WarningMessage").text().trim();
    if (warningMessage) {
        showWarningMessage(warningMessage);
    }

    // Handle Select2 package use for select and option
    $(".js-select2").select2({
        allowClear: true
    });
    $('.js-select2').on('select2:select', function (e) {
        $('form').validate().element('#' + $(this).attr('id'));
    });

    // Handle rangepicker package use for date calendar
    $(".js-rangepicker").daterangepicker({
        dateFormat: "Y-m-d",
        singleDatePicker: true,
        autoApply: true,
        "drops": "up",
        "showDropdowns": true,
        "maxDate": new Date(),

    });



});

// Initialize DataTable with export buttons and functionality
function initDataTable(varTitle) {
    $('table').DataTable({
        dom: 'Bfrtip',
        pageLength: 10, // Default page length
        lengthMenu: [10, 25, 50, 100], // Options for row counts

        columnDefs: [
            {
                targets: '.action-column', // Hide the Action column by class
                visible: false, // Hide the column in the table
                searchable: false // Ensure it's not searchable
            }
        ],
        buttons: [

            {
                extend: 'collection',
                text: '<i class=" bi bi-file-earmark-text "></i> Export',
                className: 'btn btn-sm btn-secondary dropdown-toggle',
                autoClose: true,
                buttons: [
                    {
                        extend: 'copy',
                        text: 'Copy to Clipboard',
                        title: varTitle,
                        className: 'btn btn-sm btn-secondary',
                        exportOptions: {
                            columns: ':visible:not(.action-column)'
                        }
                    },
                    {
                        extend: 'pdf',
                        text: 'Export to PDF',
                        title: varTitle,
                        className: 'btn btn-sm btn-secondary',
                        exportOptions: {
                            columns: ':visible:not(.action-column)' // Exclude the Action column by class
                        }
                    },
                    {
                        extend: 'excel',
                        text: 'Export to Excel',
                        title: varTitle,
                        className: 'btn btn-sm btn-secondary',
                        exportOptions: {
                            columns: ':visible:not(.action-column)' // Exclude the Action column by class
                        },
                        customize: function (xlsx) {
                            var sheet = xlsx.xl.worksheets['sheet1.xml'];
                            $('col', sheet).attr('width', 30); // Adjusting column width
                        }
                    },
                    {
                        extend: 'csv',
                        text: 'Export to CSV',
                        title: varTitle,
                        className: 'btn btn-sm btn-secondary',
                        exportOptions: {
                            columns: ':visible:not(.action-column)' // Exclude the Action column by class
                        }
                    },
                    {
                        extend: 'print',
                        text: 'Print',
                        title: varTitle,
                        className: 'btn btn-sm btn-secondary',
                        exportOptions: {
                            columns: ':visible:not(.action-column)' // Exclude the Action column by class
                        }
                    },
                    {
                        extend: 'colvis',
                        text: 'Column Visibility',
                        className: 'btn btn-sm btn-secondary'
                    }
                ]
            }
        ]
    });
}



// Handle Toggle status
$(document).on('click', '.js-toggle-status', function () {
    var btn = $(this);
    var Id = btn.data('id');

    // Get dynamic URL from the data attribute
    var url = btn.data('url');

    bootbox.confirm({
        message: 'Are you sure you want to toggle the status?',
        buttons: {
            confirm: {
                label: 'Yes',
                className: 'btn-danger'
            },
            cancel: {
                label: 'No',
                className: 'btn-success'
            }
        },
        callback: function (result) {
            if (result) {
                $.ajax({
                    url: url, // Use the dynamic URL
                    type: 'POST',
                    data: { Id: Id },
                    headers: {
                        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function (response) {
                        if (response.success) {
                            var row = btn.closest('tr');
                            var statusCell = row.find('td:eq(1)');
                            var currentStatus = statusCell.find('.badge').text().trim();

                            // Toggle status
                            statusCell.html('<span class="badge badge-light-' + (currentStatus === 'Available' ? 'danger' : 'success') + '">' +
                                (currentStatus === 'Available' ? 'Deleted' : 'Available') + '</span>');

                            // Update LastUpdatedOn field
                            row.find('.js-updated-on').html(response.lastUpdatedOn);
                            showSuccessMessage("The item status has been toggled successfully!");
                        } else {
                            console.error(response.message);
                            showErrorMessage("An error occurred while toggling the category status.");
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error("AJAX error:", error);
                        showErrorMessage("An error occurred while processing your request.");
                    }
                });
            }
        }
    });
});

// Handle Delete Item
$(document).on('click', '.js-delete', function () {
    var btn = $(this);
    var Id = btn.data('id');

    // Get dynamic URL and data object from data attributes
    var url = btn.data('url');

    bootbox.confirm({
        message: 'Are you sure you want to delete this item?',
        buttons: {
            confirm: {
                label: 'Yes',
                className: 'btn-danger'
            },
            cancel: {
                label: 'No',
                className: 'btn-success'
            }
        },
        callback: function (result) {
            if (result) {
                $.ajax({
                    url: url,
                    type: 'POST',
                    data: { Id: Id },
                    headers: {
                        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function (response) {
                        if (response.success) {
                            btn.closest('tr').remove();
                            showSuccessMessage("The item has been deleted successfully!");
                        } else {
                            console.error(response.message);
                            showErrorMessage("An error occurred while deleting the item.");
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error("AJAX error:", error);
                        showErrorMessage("An error occurred while processing your request.");
                    }
                });
            }
        }
    });
});



// Function to check network status
function checkNetworkStatus() {
    if (!navigator.onLine) {
        $('#submitBtn').prop('disabled', true); // Disable submit button if offline
    } else {
        $('#submitBtn').prop('disabled', false); // Enable submit button if online
    }
}

// Event listeners for online and offline events
window.addEventListener('load', checkNetworkStatus);
window.addEventListener('online', checkNetworkStatus);
window.addEventListener('offline', checkNetworkStatus);

// Function to show loading animation
function showLoading() {
    $('#submitBtn').prop('disabled', true); // Disable the button
    $('#submitBtn .loading-indicator').show(); // Show loading indicator
    $('#submitBtn i').hide(); // Hide the icon
}

// Function to hide loading animation
function hideLoading() {
    $('#submitBtn').prop('disabled', false); // Enable the button
    $('#submitBtn .loading-indicator').hide(); // Hide loading indicator
    $('#submitBtn i').show(); // Show the icon
}

function onModalBegin() {
    showLoading(); // Call to show loading when the modal begins
}

function onModalSuccess(row) {
    showSuccessMessage();
    $('#Modal').modal('hide');

    if (updatedRow !== undefined) {
        datatable.row(updatedRow).remove().draw();
        updatedRow = undefined;
    }

    var newRow = $(row);
    datatable.row.add(newRow).draw();

    KTMenu.init();
    KTMenu.initHandlers();
}

function onModalComplete() {
    hideLoading(); // Call to hide loading when the modal is complete
}


