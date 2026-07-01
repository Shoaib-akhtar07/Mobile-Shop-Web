// Admin Dashboard JavaScript

$(document).ready(function () {
    // Sidebar toggle
    $('#sidebarCollapse').on('click', function () {
        $('#sidebar').toggleClass('active');
    });

    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // Data table sorting
    $('.table-sortable th').on('click', function () {
        var table = $(this).parents('table').eq(0);
        var rows = table.find('tr:gt(0)').toArray().sort(comparer($(this).index()));
        this.asc = !this.asc;
        if (!this.asc) {
            rows = rows.reverse();
        }
        for (var i = 0; i < rows.length; i++) {
            table.append(rows[i]);
        }
    });

    function comparer(index) {
        return function (a, b) {
            var valA = getCellValue(a, index);
            var valB = getCellValue(b, index);
            return $.isNumeric(valA) && $.isNumeric(valB) ? valA - valB : valA.toString().localeCompare(valB);
        };
    }

    function getCellValue(row, index) {
        return $(row).children('td').eq(index).text();
    }

    // Confirm delete
    $('.confirm-delete').on('click', function (e) {
        if (!confirm('Are you sure you want to delete this item?')) {
            e.preventDefault();
        }
    });

    // Bulk actions
    $('#selectAll').on('change', function () {
        $('.select-item').prop('checked', $(this).prop('checked'));
    });

    // Image preview
    window.previewImage = function (input, previewId) {
        if (input.files && input.files[0]) {
            var reader = new FileReader();
            reader.onload = function (e) {
                $('#' + previewId).attr('src', e.target.result).show();
            };
            reader.readAsDataURL(input.files[0]);
        }
    };

    // Dynamic form fields
    window.addSpecificationField = function () {
        var html = `
            <div class="row g-3 specification-row mb-3">
                <div class="col-md-4">
                    <input type="text" name="specNames[]" class="form-control" placeholder="Name" required />
                </div>
                <div class="col-md-4">
                    <input type="text" name="specValues[]" class="form-control" placeholder="Value" required />
                </div>
                <div class="col-md-3">
                    <input type="text" name="specGroups[]" class="form-control" placeholder="Group" />
                </div>
                <div class="col-md-1">
                    <button type="button" class="btn btn-danger btn-sm" onclick="$(this).closest('.specification-row').remove()">
                        <i class="bi bi-trash"></i>
                    </button>
                </div>
            </div>
        `;
        $('#specificationsContainer').append(html);
    };

    // Order status update
    window.updateOrderStatus = function (orderId, status) {
        $.post('/Admin/Orders/UpdateStatus', { id: orderId, status: status }, function (data) {
            if (data.success) {
                location.reload();
            }
        });
    };

    // Export table to CSV
    window.exportTableToCSV = function (tableId, filename) {
        var csv = [];
        var rows = document.querySelectorAll('#' + tableId + ' tr');

        for (var i = 0; i < rows.length; i++) {
            var row = [];
            var cols = rows[i].querySelectorAll('td, th');

            for (var j = 0; j < cols.length; j++) {
                row.push(cols[j].innerText);
            }

            csv.push(row.join(','));
        }

        downloadCSV(csv.join('\n'), filename);
    };

    function downloadCSV(csv, filename) {
        var csvFile = new Blob([csv], { type: 'text/csv' });
        var downloadLink = document.createElement('a');
        downloadLink.download = filename;
        downloadLink.href = window.URL.createObjectURL(csvFile);
        downloadLink.style.display = 'none';
        document.body.appendChild(downloadLink);
        downloadLink.click();
        document.body.removeChild(downloadLink);
    }

    // Auto-hide alerts
    setTimeout(function () {
        $('.alert-dismissible').alert('close');
    }, 5000);

    // Chart.js defaults
    if (typeof Chart !== 'undefined') {
        Chart.defaults.responsive = true;
        Chart.defaults.maintainAspectRatio = false;
        Chart.defaults.plugins.legend.position = 'bottom';
    }

    // Date range picker
    $('.date-range').on('change', function () {
        var startDate = $('#startDate').val();
        var endDate = $('#endDate').val();

        if (startDate && endDate) {
            window.location.href = window.location.pathname + '?startDate=' + startDate + '&endDate=' + endDate;
        }
    });

    // Stock alert
    window.checkStock = function (productId) {
        $.get('/Admin/Products/CheckStock/' + productId, function (data) {
            if (data.stock <= 10) {
                showAlert('Low stock alert: Only ' + data.stock + ' units remaining!', 'warning');
            }
        });
    };

    // Print function
    window.printInvoice = function () {
        window.print();
    };

    // Responsive sidebar
    function handleResize() {
        if ($(window).width() <= 768) {
            $('#sidebar').addClass('active');
        } else {
            $('#sidebar').removeClass('active');
        }
    }

    $(window).resize(handleResize);
    handleResize();
});

// Utility functions
window.showAlert = function (message, type) {
    var alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    $('.container-fluid').prepend(alertHtml);

    setTimeout(function () {
        $('.alert').alert('close');
    }, 5000);
};

window.formatCurrency = function (amount) {
    return 'RS ' + parseFloat(amount).toLocaleString('en-IN', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });
};

window.formatDate = function (dateString) {
    var date = new Date(dateString);
    return date.toLocaleDateString('en-IN', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
    });
};