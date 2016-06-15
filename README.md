# ILDependencyGraph

This console app produces a dot graph of directly and indirectly referenced dependencies of a provided .NET assembly file (exe/dll)

**It only shows dependencies that are dristributed together with the assembly in the same directory (or optionally you can specify a separate directory)**

This means that mscorlib.dll and many other libs will not be shown here (by design).

# Example
You can use the text output to render the diagram via Graphviz.

One way (there are many) of producing a rendered graph on Windows would be:
- ILDependencyGraph.exe "path/to/my/assembly/IlSpy.exe" > assembly.dot
- Open GVEdit that comes with Graphviz for Windows and load the assembly.dot

![](https://cdn.rawgit.com/pingec/ILDependencyGraph/master/images/IlSpy.svg)