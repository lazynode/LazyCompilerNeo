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
    <a:string val="Hello, world!" />
</lazy>
```

* the root node must be **lazy**
* attribute in the root node `xmlns:a="Assembly"` defines the namesapce **a**, whose URI is **Assembly**
* there are two URIs for now (**Assembly** and **Basic**) which contains different sugers and features
* the node **string** under **Assembly** provides a syntax suger for easily putting a string literal on the neo-vm's stack

Let's run the compiler and see the result.

```sh
> cat examples/1.helloworld.xml | dotnet run . | base64

DA1IZWxsbywgd29ybGQh
```

* the compiler takes standard input as source code and print the bytecode into standard output in binary format
* as NEO use the base64 format everywhere, we'd better convert it into base64 for next step

## Debug

There exists an envirment variable called `DEBUG`. After setting it, the compiler will output the opcode before translating into bytecode.

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

A full list of opcode can be found [here](https://docs.neo.org/docs/en-us/reference/neo_vm.html).


