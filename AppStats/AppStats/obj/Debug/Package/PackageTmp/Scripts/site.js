


(function ($) {
    $.fn.cascade = function (options) {
        var defaults = {};
        var opts = $.extend(defaults, options);

        return this.each(function () {
            $(this).change(function () {
                var selectedValue = $(this).val();
                var params = {};

                params["LanguageId"] = $("#Language.FilterElement> .Selector > Select", $(this).closest(".FilterBox"))[0].value;
                params["EnvironmentId"] = $("#Environment.FilterElement> .Selector > Select", $(this).closest(".FilterBox"))[0].value;
                params["ProcCount"] = $("#ProcCount.FilterElement> .Selector > Select", $(this).closest(".FilterBox"))[0].value;

                $.getJSON(opts.url, params, function (items) {
                    if (items != null) {
                        opts.childSelect.empty();
                        var val;
                        $.each(items, function (index, item) {
                            val = item.Value;
                            opts.childSelect.append(
                                $('<option/>')
                                    .attr('value', item.Value)
                                    .text(item.Text)
                            );
                        });

                        if (items.length == 1) {
                            $(opts.childSelect).change();
                        }
                    }
                });
            });
        });
    };
})(jQuery);
