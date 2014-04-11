ModalContentPresenter
=====================

Easily display modal content in WPF applications.

What is it?
-----------

The `ModalContentPresenter` is a custom `FrameworkElement` that allows you to display modal content in WPF applications.
The control adheres to WPF best practices by providing dependency properties for binding and routed events for reacting to state changes in the control.

The control can be used like this:

<c:ModalContentPresenter IsModal="{Binding DialogIsVisible}">
    <TabControl Margin="5">
            <Button Margin="55"
                    Padding="10"
                    Command="{Binding ShowModalContentCommand}">
                This is the primary Content
            </Button>
        </TabItem>
    </TabControl>

    <c:ModalContentPresenter.ModalContent>
        <Button Margin="75"
                Padding="50"
                Command="{Binding HideModalContentCommand}">
            This is the modal content
        </Button>
    </c:ModalContentPresenter.ModalContent>

</c:ModalContentPresenter>

Features:

 - Displays arbitrary content.
 - Does not disable the primary content whilst the modal content is being displayed.
 - Disables mouse and keyboard access to the primary content whilst the modal content is displayed.
 - Is only modal to the content it is covering, not the entire application.
 - can be used in an MVVM friendly way by binding to the IsModal property.
