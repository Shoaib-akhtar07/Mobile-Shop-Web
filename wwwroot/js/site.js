// MobileShop Site JavaScript

$(document).ready(function () {
    // Search Auto-suggestions
    var searchTimeout;
    $('#searchInput').on('input', function () {
        clearTimeout(searchTimeout);
        var term = $(this).val();

        if (term.length < 2) {
            $('#searchSuggestions').hide();
            return;
        }

        searchTimeout = setTimeout(function () {
            $.get('/Products/Search', { term: term }, function (data) {
                if (data.length > 0) {
                    var html = '';
                    data.forEach(function (item) {
                        html += `<a href="/Products/Details/${item.id}" class="list-group-item list-group-item-action d-flex align-items-center">
                            <img src="${item.image || 'https://via.placeholder.com/40x40?text=No+Image'}" class="rounded me-3" style="width: 40px; height: 40px; object-fit: cover;" />
                            <div>
                                <h6 class="mb-0">${item.name}</h6>
                                <small class="text-muted">${item.brand} - RS ${item.price.toLocaleString()}</small>
                            </div>
                        </a>`;
                    });
                    $('#suggestionsList').html(html);
                    $('#searchSuggestions').show();
                } else {
                    $('#searchSuggestions').hide();
                }
            });
        }, 300);
    });

    // Hide suggestions when clicking outside
    $(document).on('click', function (e) {
        if (!$(e.target).closest('#searchInput').length) {
            $('#searchSuggestions').hide();
        }
    });

    // Wishlist functionality
    window.addToWishlist = function (productId) {
        $.post('/Account/AddToWishlist', { productId: productId }, function (data) {
            if (data.success) {
                showToast(data.message, 'success');
            }
        }).fail(function () {
            window.location.href = '/Account/Login';
        });
    };
    // Toast notifications
    window.showToast = function (message, type) {
        var bgClass = type === 'success' ? 'bg-success' : type === 'error' ? 'bg-danger' : 'bg-info';
        var toast = $(`<div class="toast align-items-center ${bgClass} text-white position-fixed" role="alert" style="top: 20px; right: 20px; z-index: 9999;">
        <div class="d-flex">
            <div class="toast-body">${message}</div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
        </div>
    </div>`);

        $('body').append(toast);
        toast.toast({ delay: 3000 });
        toast.toast('show');

        setTimeout(function () {
            toast.remove();
        }, 3500);
    };
});