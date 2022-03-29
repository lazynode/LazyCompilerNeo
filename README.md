# NEO COMPILER

Yet another NEO compiler.

Based on XML parser, this compiler could translate original NEO opcode into bytecode. Plus, it provides some syntax suger for esay usage. What's more, we provide some high level features such as conditions, loops as well as functions.

* *high performance* as raw opcode
* *automatic address calculation* for loops and conditions opcode
* *high level features* such as function

We assume you know the neo-vm's composition and principles. If not, we recommend you read [docs](https://docs.neo.org/docs/en-us/basic/neovm.html) first.

## Basic Usage

As a convention, let's write a `Hello, world!` first.

```xml
<lazy xmlns:a="Assembly">
    <!-- This program left a **ByteString** "Hello, world!" on the **ResultStack**. -->
    <a:string val="Hello, world!" />
</lazy>
```

* the root node must be **lazy**
* attribute in the root node `xmlns:a="Assembly"` defines the namesapce **a**, whose URI is **Assembly**
* there are two URIs for now (**Assembly** and **Basic**) which contains different sugers and features
* the node **string** under **Assembly** provides a syntax suger for easily putting a string literal on the neo-vm's stack
* comments is supportted by XML itself
* `lazy` node can also be used for code formatting, its usage is the same as `{` and `}` in C-like programming languages.

Let's run the compiler and see the result.

```sh
> cat examples/1.helloworld.xml | dotnet run . | base64

DA1IZWxsbywgd29ybGQh
```

* the compiler takes standard input as source code and print the bytecode into standard output in binary format
* as NEO use the base64 format everywhere, we'd better convert it into base64 for next step

## Debug

There exists an envirment variable called `DEBUG`. After setting that, the compiler will output the opcode before translating it into bytecode.

```sh
> DEBUG=1 cat examples/1.helloworld.xml | dotnet run . | base64

<lazy xmlns:a="Assembly">
  <pushdata1 oprand="0d48656c6c6f2c20776f726c6421" />
</lazy>
DA1IZWxsbywgd29ybGQh
```

As you can see, the debug output will not contain any node under the two namespaces. Only the opcode supportted by neo-vm directly will show here.

Of cause, this xml document is also a correct code version for printing `Hello, world!` while it is hard for human reading.

## Evaluation

```sh
> curl http://seed1.neo.org:20332 -d '{ "jsonrpc": "2.0", "id": 1, "method": "invokescript", "params": ["DA1IZWxsbywgd29ybGQh"] }'| json_pp

{
   "id" : 1,
   "jsonrpc" : "2.0",
   "result" : {
      "exception" : null,
      "gasconsumed" : "24",
      "script" : "DA1IZWxsbywgd29ybGQh",
      "stack" : [
         {
            "type" : "ByteString",
            "value" : "SGVsbG8sIHdvcmxkIQ=="
         }
      ],
      "state" : "HALT"
   }
}
```

We can execute the bytecode by sending it to neo RPC node. More docs can be found [here](https://docs.neo.org/docs/en-us/reference/rpc/latest-version/api/invokescript.html).

```
> echo 'SGVsbG8sIHdvcmxkIQ==' | base64 -d

Hello, world!
```

Then parse the result on the result stack. We'll get the output.

## Node Usage

There are three types of nodes for now: Raw opcode, syntax sugers in namespace `Assembly` and features in namesapce `Basic`.

### opcode node

A full list of opcode can be found [here](https://docs.neo.org/docs/en-us/reference/neo_vm.html).

The opcode node contains only one optional attribute called **oprand**. Every node can not contain a body.

As list in the document, some opcode do not need any parameter, directly use them without attribute will be OK. Some opcode need an oprand standing for specific meanings, then the node must contain a attribute will its value in *little-endian hex encoded format*. The format is used [here](https://github.com/neo-project/neo-vm/blob/5d6b5fed6b3e140d0a2b9b6a3cfc4720651d7e50/src/neo-vm/Instruction.cs#L79) in neo-vm's code.

Take the `pushdata1` as example.

> PUSHDATA1, PUSHDATA2, PUSHDATA4: The next n bytes contain the number of bytes to be pushed onto the stack, where n is specified by 1|2|4.

This means the next 1 byte oprand is used for string data length x. The next x bytes stands for the data.

Therefore, the first byte of the oprand should be 13 (0x0d) for `Hello, world!`. The following bytes should be hex encoded string. One solution is `echo -n Hello, world! | od -A n -t x1`. Therefore, the code should be `<pushdata1 oprand="0d48656c6c6f2c20776f726c6421" />` in above code.

### Assembly node

All syntax sugers are showed below. Those attributes who have default values will be list in `${attri}=${default}` format in the second column. Those nodes who can have a body will be showed in `<${node}><lazy/></${node}>` format in the third column. The `<lazy/>` in the example column could be any other nodes instead.

Attribute `cond`'s value can be chosen from `if`, `ifnot`, `eq`, `ne`, `gt`, `ge`, `lt`, `le`. **Note!!! Note!!! Note!!!** Those who contains `cond` will pop the value from the stack, please `<dup/>` the value if you want to use it again.

node  | attributes | example          | description
----- | ---------- | ---------------  | -------
int   | val        | `<a:int val="1">`  | put an **Integer** literal onto the evaluation stack
string| val        | `<a:string val="Hello, world!">` | put an **ByteString** literal onto the evaluation stack
bytes | val        | `<a:bytes val="48656c6c6f">`  | put an **Buffer** literal onto the evaluation stack
bool  | val        | `<a:bool val="true">`  | put an **Boolean** literal onto the evaluation stack
 | | |
dowhile | cond=if | `<a:dowhile><lazy/></a:dowhile>` | this constructs a dowhile loop in its body, in the end of every loop, it will pop a value on the stack, if it's true, the loop will continue
while | cond=if | `<a:while cond="eq"><lazy/></a:while>` | this constructs a while loop in its body, in the start of every loop, it will pop two values on the stack, if they're equal, the loop will continue
if | - | `<a:if><lazy/></a:if>` | pop a value on the stack, if it's true, the body will be executed, vice versa
else | - | `<a:else><lazy/></a:else>` | must used after the `if` node, will be executed if the value is not true
 | | |
syscall | name | `<a:syscall name= "System.Storage.Get" />` | this will construct a system call, the name can be chosen from [here](https://docs.neo.org/docs/en-us/reference/scapi/interop.html)
contractcall | flag=All,method,hash | `<a:contractcall hash="${STDLIB'S HASH}" method="jsonSerialize" />` | a convenience format for syscall `System.Contract.Call`, the example calls the stdlib's json serialize method, the hash hould be 0xacce6fd80d44e1796aa0c2c625e9e4e0ce39efc0

There also exist tree other nodes (goto,invoke,removable), which we do not recommend using. If you use, you'd better understand the compiler's code.
