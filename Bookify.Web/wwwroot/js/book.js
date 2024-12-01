$(document).ready(function () {
    var table = $('#booksTable').DataTable({
        "processing": true,
        "serverSide": true,
        "ajax": {
            "url": "/books/GetBooksData", // Adjust URL based on your routing
            "type": "POST",
            "datatype": "json"
        },
        "columns": [

            {
                "data": "title",
                "name": "Title",
                "autoWidth": true,
                "render": function (data, type, row) {
                    return `<a href="/Books/Details/${row.id}" style="color: inherit; text-decoration: none;">${data}</a>`;
                }
            },
            { "data": "authorName", "name": "Author", "autoWidth": true },
            { "data": "publisher", "name": "Publisher", "autoWidth": true },
            {
                "data": "isDeleted",
                "name": "Status",
                "autoWidth": true,
                "render": function (data) {
                    return data
                        ? '<span class="badge badge-danger">Deleted</span>'
                        : '<span class="badge badge-success">Available</span>';
                }
            },
            { "data": "createdOn", "name": "CreatedOn", "autoWidth": true },
            { "data": "lastUpdated", "name": "LastUpdated", "autoWidth": true },
            {
                "data": null,
                "name": "Actions",
                "orderable": false,
                "className": 'text-end',
                "render": function (data, type, row) {
                    return `
                                                <div class="dropdown">
                                                    <a href="#" class="btn btn-light btn-active-light-primary btn-sm dropdown-toggle" data-bs-toggle="dropdown" aria-expanded="false">
                                                        Actions
                                                    </a>
                                                    <ul class="dropdown-menu">
                                                        <li><a class="dropdown-item" href="/Books/Edit/${row.id}">Edit</a></li>
                                                                <li><a class="dropdown-item js-toggle-status" href="/Books/Details/${row.id}">Details </a></li>
                                                                <li><a class="dropdown-item js-delete-book" href="javascript:;" data-id="${row.id}" data-url="/Books/Delete">Delete</a></li>
                                                            </ul>
                                                </div>`;
                }
            }
        ]

    });

    // Search functionality
    $('#searchInput').on('keyup', function () {
        table.search(this.value).draw();
    });

    // Event handler for delete button
    $('#booksTable tbody').on('click', '.js-delete-book', function () {
        var btn = $(this);
        HandleDelete(btn); // Pass the button element to HandleDelete
    });
});