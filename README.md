# IxiWatt - Easy IxiCash Miner
IxiWatt converts your electricity and your computer's processing power into IxiCash.


## About Ixian

Ixian DLT is a revolutionary blockchain that brings several innovative advantages, such as processing a high volume of micro-transactions quickly while consuming a low amount of processing power, disk space and energy.

**Homepage**: https://www.ixian.io

**Discord**: https://discord.gg/pdJNVhv

**Bitcointalk**: https://bitcointalk.org/index.php?topic=4631942.0

**Documentation**: https://docs.ixian.io

## The repository

The Ixian GitHub project is divided into seven main parts:

* [Ixian-Core](https://github.com/ProjectIxian/Ixian-Core): Functionality common to all other projects.
* [Ixian-DLT](https://github.com/ProjectIxian/Ixian-DLT): Implementation of the blockchain-processing part (the Master Node software).
* [Ixian-S2](https://github.com/ProjectIxian/Ixian-S2): Implementation of the streaming network (the S2 Node software).
* [Spixi](https://github.com/ProjectIxian/Spixi): Implementation of the SPIXI messaging client for Windows, Android and iOS.
* [Ixian-Miner](https://github.com/ProjectIxian/Ixian-Miner): Implementation of the Ixian standalone mining software.
* [Ixian-LiteWallet](https://github.com/ProjectIxian/Ixian-LiteWallet): Simple CLI wallet for the Ixian DLT network.
* [Ixian-Pool](https://github.com/ProjectIxian/Ixian-Pool): Mining pool software.


## How To Use
1. Start IxiWatt application.  
2. Select pool from the selection box or enter a custom pool address in Pool URL input field.  
3. Enter your IxiCash wallet address into the IxiCash Wallet Address input field.  
4. Set intensity to the desired level and start mining.  

If the software crashes with missing DLL/files exception, install .NET Framework 4.8 and the VC Redistributable 2013 and 2015. VC Redist files are included with every IxiWatt release in the vcredist folder.

## Tips
**Never mine directly to an exchange's address!**  
  
Play with intensity setting (try with 10-20% first, to get an estimate, then try higher values), it's probable that lower intensity will give you better results.  
For example we've noticed that the sweet spot for most AMD GPUs tested is around 27%.  


We don't recommend using higher intensity values than 50%, unless you know what you're doing.  

You want as much hash rate as you can get and as little rejected shares as possible.  

Note that intensity setting determines allowed usage of your CPU/GPU by the miner.  

Higher settings will use your computer more and it will heat more, potentially reducing the life-span of your computer.  

Happy Mining!  


## Dev fees
In order to support development, this miner has 1.5% dev fee included. Every 100 minutes of mining, it will mine 1.5 minutes for developers.  


## Where to get IxiCash Wallet Address?
The easiest way to get an IxiCash Address is with Spixi (Decentralized chat app and more) or by using an Ixian LiteWallet.  


## Building from source
### Windows
Visual Studio 2019 is required (Community Edition is fine), you can get it from here: [Visual Studio](https://visualstudio.microsoft.com/)

Several NuGetPackages are downloaded automatically during the build process.

## Development branches

There are two main development branches:
* master: This branch is used to build the binaries for the releases. It should change slowly and be quite well-tested. This is also the default branch for anyone who wishes to build their Ixian software from source.
* development: This is the main development branch and the source for testnet binaries. The branch might not always be kept bug-free, if an extensive new feature is being worked on. If you are simply looking to build a current testnet binary yourself, please use one of the release tags which will be associated with the master branch.

## Documentation

You can find documentation on how to build, APIs and other documents on [Ixian Documentation Pages](https://docs.ixian.io).

## Get in touch / Contributing

If you feel like you can contribute to the project, or have questions or comments, you can get in touch with the team through Discord: (https://discord.gg/dbg9WtR)

## Pull requests

If you would like to send an improvement or bugfix to this repository, but without permanently joining the team, follow these approximate steps:

1. Fork this repository
2. Create a branch (preferably with a name that describes the change)
3. Create commits (the commit messages should contain some information on what and why was changed)
4. Create a pull request to this repository for review and inclusion.
