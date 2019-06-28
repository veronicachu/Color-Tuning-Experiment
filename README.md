# Color-Tuning-Experiment
## Unity/C# code to create visual search experiment probing color feature attention filters
### University of California, Irvine

### Author: Veronica C. Chu

----

### Overview:
Experimental framework for a feature-based attention study using visual search to ensure attention to color. Three different stimuli hue values can be set by the experimenter in DKL color space; lookup table values from monitor spectrophotometer measurements must be added as csv files. During an initial calibration phase of the experiment, subjects can adjust luminance and saturation values of central stimuli colors. The each experiment trial presents multiple visual search arrays containing 3 colors (1 target color and 2 distractor colors). Subjects count the number of target appearances within the trial and enters the counted number in an input box at the end of the trial. While subjects attend to the central visual search task, the peripheral flickers various colors trial-to-trial to probe at the subject's attentional color filter using measurements of SSVEP responses.

### Experiment Build:
To see program, download 'Color Tuning SSVEP' folder, and run 'Color Tuning SSVEP 1.exe'.
Instructions:
1. Hit 'spacebar' key to continue from "Wait for Experimenter Instructions"
2. A 'T' will appear - this is the exact target color and orientation to count the number of during the visual search task
3. Hit 'numpad enter' key to move on to the visual search task
4. Four randomly generated visual search arrays will appear, count the number of times the target 'T' appears
5. Enter the counted number in the input field using either the numpad
6. Repeat the steps above until the end

**There are 6 practice trials in the beginning and 100 experimental trials, separated by a "Wait for Experiment Instructions" page
**Colors may not be the same ones used in the experiment as apperance will vary with different monitors
