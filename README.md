# System Resources Control WPF

## Description

This WPF application provides information about your computer's system resources, such as CPU, GPU, RAM, disks, network, and temperature. It allows users to monitor the state of their hardware.

## Features

*   **CPU Info:** Displays detailed information about the processor, including name, manufacturer, number of cores and threads, clock speed, and cache.
*   **GPU Info:** Shows information about graphics adapters, including name, memory, driver version, and video processor.
*   **RAM Info:** Provides information about the installed RAM, including total capacity, as well as details about each memory module.
*   **Disk Info:** Displays information about hard disks, including model, interface type, size, and media type.
*   **Network Info:** Shows information about network adapters, including name, MAC address, and speed.
*   **System Summary:** Displays a brief summary of information about CPU, RAM, GPU, and disks.
*   **Temperature Info:** Shows the current temperature.
*   **Benchmark feature:** For performance evaluation
*   **Controls:**
    *   "Continue (y)" button: Restarts monitoring.
    *   "Stop (n)" button: Stops monitoring.
    *   "Exit" button: Closes the application.

## Technologies

*   **Language:** C#
*   **Framework:** WPF (.NET Framework or .NET Core/.NET)
*   **Libraries:**
    *   System.Management (for obtaining system information)
*   **UI**: Basic WPF controls.

## Build and Run

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/illusiOxd/SRCapp.git
    ```
2.  **Open the project:** Open the `.csproj` file in Visual Studio or Rider.
3.  **Install NuGet packages:** Install the `System.Management` NuGet package.
4.  **Build the project:** Select "Build" -> "Build Solution" (or "Rebuild Solution").
5.  **Run the project:** Click the "Start" button (or `F5` / `Ctrl+F5` in Visual Studio).

## How to use

1.  After launching the application, the main window with buttons will appear.
2.  Click the corresponding button to get information about the desired resource.
3.  The information will be displayed in the window as a list of items.
4.  Use the "Continue (y)" and "Stop (n)" buttons to manage monitoring, or "Exit" to close the application.

## Future Plans

*   Potential improvements to the UI and organization of information.

## Contact

If you have questions or suggestions, contact me via email: \gamertimka9@gmail.com

## License

MIT License

Copyright (c) 2024 Timothy

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
