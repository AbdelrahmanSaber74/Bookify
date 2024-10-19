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
                targets: '.js-no-export', // Hide the Action column by class
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
                            columns: ':visible:not(.js-no-export)'
                        }
                    },
                    {
                        extend: 'pdf',
                        text: 'Export to PDF',
                        title: varTitle,
                        className: 'btn btn-sm btn-secondary',
                        exportOptions: {
                            columns: ':visible:not(.js-no-export)' // Exclude the Action column by class
                        }
                    },
                    {
                        extend: 'excel',
                        text: 'Export to Excel',
                        title: varTitle,
                        className: 'btn btn-sm btn-secondary',
                        exportOptions: {
                            columns: ':visible:not(.js-no-export)' // Exclude the Action column by class
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
                            columns: ':visible:not(.js-no-export)' // Exclude the Action column by class
                        }
                    },
                    {
                        extend: 'print',
                        text: 'Print',
                        title: varTitle,
                        className: 'btn btn-sm btn-secondary',
                        exportOptions: {
                            columns: ':visible:not(.js-no-export)' // Exclude the Action column by class
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
$('body').delegate('.js-toggle-status', 'click', function () {
    var btn = $(this);
    var id = btn.data('id'); // Retrieve the data-id attribute value

    bootbox.confirm({
        message: "Are you sure that you need to toggle this item status?",
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
        callback: function (result) {
            if (result) {
                $.post({
                    url: btn.data('url'),
                    data: {
                        '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val(),
                        id: id // Correctly placed comma here
                    },
                    success: function (response) { // Use response instead of lastUpdatedOn
                        var row = btn.parents('tr');
                        var status = row.find('.js-status');
                        var newStatus = status.text().trim() === 'Deleted' ? 'Available' : 'Deleted';

                        // Update the status text and classes
                        status.text(newStatus).toggleClass('badge-light-success badge-light-danger');

                        // Update the LastUpdatedOn field assuming it’s in the response
                        row.find('.js-updated-on').html(response.lastUpdatedOn);

                        row.addClass('animate__animated animate__flash');

                        // Show success message
                        showSuccessMessage("The item status has been toggled successfully!");
                    },
                    error: function () {
                        showErrorMessage("An error occurred while toggling the item status.");
                    }
                });
            }
        }
    });
});


$('body').delegate('.js-delete', 'click', function () {
    var btn = $(this); // Define btn using the clicked element
    var Id = btn.data('id'); // Get the ID
    var url = btn.data('url'); // Get the dynamic URL

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
                    data: { id: Id },
                    headers: {
                        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function (response) {
                        if (response.success) {
                            btn.closest('tr').remove(); // Remove the row from the DataTable
                            showSuccessMessage("The item has been deleted successfully!");

                        } else {
                            console.error("Error Message:", response.message);
                            console.error("Detailed Error:", response.errorMessage);
                            showErrorMessage(response.message || "An error occurred while deleting the item.");
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




// Define a named function for handling the modal rendering
function attachModalEvent() {
    $('.js-render-modal').on('click', function (e) {
        e.preventDefault();
        var title = $(this).data('title');
        var actionUrl = $(this).attr('href'); // Get the action URL from the link

        // Set the modal title
        $('#copyModalLabel').text(title);

        // Load the form into the modal body
        $.ajax({
            url: actionUrl,
            type: 'GET',
            success: function (data) {
                $('#copyModal .modal-body').html(data);
                $('#copyModal').modal('show');
            },
            error: function (xhr, status, error) {
                // Handle errors here
                console.error(error);
            }
        });
    });
}



//Handle Confirm
$('body').delegate('.js-confirm', 'click', function () {
    var btn = $(this); 

    bootbox.confirm({
        message: btn.data('message'),  
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
                // If user confirms, send POST request to unlock the user
                $.post({
                    url: btn.data('url'), 
                    data: {
                        '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() 
                    },
                    success: function () {
                        showSuccessMessage("User has been unlocked successfully!");
                    },
                    error: function () {
                        showErrorMessage("Failed to unlock the user. Please try again.");
                    }
                });
            }
        }
    });
});
