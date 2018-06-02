# UNIX line endings

[Web site](https://unixlineendings.experimentalcommunity.org)

[Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=hack2root.unixlineendings)

Converts from Windows (CRLF) to UNIX (LF) line endings style using Visual Studio

UNIX line endings  extension provides integrated support for replacing CR LF with LF in open files

Featured:

- Tools menu item
- Tool window output
- Undo/redo
- Keyboard shortcuts

Main feature is that this extension uses internal COM+ interfaces and do not touches files on a disk. Imagine this is a recorded macro for text replacement and deployed as VSIX bundle. Implementation works internally through DTE2 interface, so you never get undebaggable code due to source code mismatch.

You can also assign command binding for this extension as Alt + N keyboard macro for fast user experience