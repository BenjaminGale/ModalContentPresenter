ModalContentPresenter
=====================

Easily display modal content in WPF applications.

What is it?
-----------

The `ModalContentPresenter` is a custom `FrameworkElement` that allows you to display modal content in WPF applications.
The control adheres to WPF best practices by providing dependency properties for binding and routed events for reacting to state changes in the control.

The control can be used like this:

```
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
```

Features:

 - Displays arbitrary content.
 - Does not disable the primary content whilst the modal content is being displayed.
 - Disables mouse and keyboard access to the primary content whilst the modal content is displayed.
 - Is only modal to the content it is covering, not the entire application.
 - can be used in an MVVM friendly way by binding to the IsModal property.
 
Refer to my [blog post](http://programmingwithpassion.wordpress.com/2012/07/01/displaying-modal-content-in-wpf/) for further details.
 
Developer notes
---------------

Note that the `ModalContentPresenter` derives from `FrameworkElement` rather than `Control` or `ContentControl`. The reason for this is that the other options allow the control to be re-skinned which didn't seem approriate to me because the `ModalContentPresenter` provides a behaviour rather than a look and feel. Deriving from `FrameworkElement` allowed me to effectivly hide the details of the internal structure of the control and stop the user from altering it.

Deriving from `FrameworkElement`, however, brings it's own set of challenges. There are a lot of details that you **have** to implement yourself. This means the source code is a good example of how to implement your own custom `FrameworkElement`. I wrote a [blog](http://programmingwithpassion.wordpress.com/2012/07/03/custom-framework-elements/) post to accompany the control to record what I learnt whilst writing this control.


