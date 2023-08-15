% ultrasonic needs 5V



% Software to install:

% Home>Add-Ons>Get Add-Ons> MATLAB Support Package for Arduino Hardware by
% MathWorks
% Home>Add-Ons>Get Add-Ons> Legacy MATLAB and Simulink Support for Arduino
% by Giampiero Campa

% Arduino Setup
% Apps>Arduino Explorer>Configure Arduino over USB>Hardware Setup (upper LH
% corner)
% Select Uno for board, pick whatever COM and adjust it in this code, and
% select libraries 'Ultrasonic', 'SPI', 'Servo', 'I2C'
% click program. It'll take at least 10-60 seconds
% click next twice and finish
% congrats, it is now time for this program

% adjust the COM on line 24

delay = .5; %.015 minimum. This is the delay between when the servo starts moving again and taking another servo sampling
g=1;

arduinoObj = arduino('COM10', 'Uno', 'Libraries', {'Ultrasonic', 'Servo'}); % defines arduino and Arduino libraries in MATLAB. Check that the COM is correct
ultrasonicObj = ultrasonic(arduinoObj, 'D11', 'D12', 'OutputFormat', 'double'); % defines ultrasonic sensor and its pins

servo1 = servo(arduinoObj, 'D10', 'MinPulseDuration', 0.0015, 'MaxPulseDuration', 0.0019);
servo2 = servo(arduinoObj, 'D13', 'MinPulseDuration', 0.0015, 'MaxPulseDuration', 0.0019); % cancel servo2 line

z=1; %used to make in infinitely-running code
while z>0
    if g<4 %3 baselines are taken because sometimes the sound wave will bounce off and return a different value at a corner (up to 1/3 of the time). this redundancy will make sure we double-check against a few data sets for true detection
        a=readDistance(ultrasonicObj) / 2.54 * 100 % Call the baseline subroutine
     
    else
       % USlive=scan(arduinoObj,ultrasonicObj,servo1,delay,USbaseline1,USbaseline2,USbaseline3);
        %USlive=USlive
    end
   % g=g+1;
end




function [USbaseline] = baseline(arduinoObj, ultrasonicObj, servo1, servo2, delay)
    USbaseline = zeros(13);
    for row = 1:13

        %writePosition(servo2,(row-1)*15/180)  %cancel servo2 ajusts phi after all of theta has been scanned
        writePosition(servo2,0+15/180)
        if rem(row, 2) == 1    %when the servo is moving left-to-right
            for theta = 0:15/180:1
                % in steps of 15 degrees because that is the sonar beam
                % width
                writePosition(servo1, theta); % tell servo to go to position in variable 'pos'
                pause(delay);
                column = int32(theta * 180/15) + 1;
                USbaseline(row, column) = readDistance(ultrasonicObj) / 2.54 * 100; %reads dist. from US in inches
            end
        end

        if rem(row, 2) == 0 %when the servo is moving right-to-left
            for pos = 1:-15/180:0       
                % in steps of 1 degree
                writePosition(servo1, pos); % tell servo to go to position in variable 'pos'
                pause(delay);
                column = int32(pos * 180/15) + 1;
                USbaseline(row, column) = readDistance(ultrasonicObj) / 2.54 * 100;
            end
        end

    end %end of scan

    writePosition(servo1, 0); %reset to starting position
    writePosition(servo2, 0); % cancel servo2
end %end of function

% clear arduinoObj angle distance servo1 current_pos ultrasonicObj USbaseline USlive dummy a pos servo2
% baseline=baseline

%.1728 ~6in ~15cm
% setup:
% for(j,1,12) j = vertical angle phi, this adjusts the level

function [USlive]=scan(arduinoObj,ultrasonicObj,servo1,delay,USbaseline1,USbaseline2,USbaseline3)

    for row = 1:13
            % writePosition(servo2,(a-1)*15/180) %cancel servo2 3d shift

            if rem(row, 2) == 1
             for pos = 0:15/180:1
                    % in steps of 1 degree
                    writePosition(servo1, pos); % tell servo to go to position in variable 'pos'
                    pause(delay);
                    column = int32(pos * 180/15) + 1;

                    USlive(row, column) = readDistance(ultrasonicObj) / 2.54 * 100;
                    w=abs(USlive(row,column)-USbaseline1(row,column));
                    x = abs(USlive(row,column)-USbaseline2(row,column));
                    y= abs(USlive(row,column)-USbaseline3(row,column));

                    if w > (.4 .* USbaseline1(row,column) ) & x > (.4 .* USbaseline2(row,column) ) & y > (.4 .* USbaseline1(row,column) )
                        %if the scan deviated over 40% from the 3 baselines
                        detection(3) = (row-1)*15; %phi 
                        detection(2) = (column-1)*15; %theta
                        detection(1) = USlive(row,column); %distance
                        detection=detection %outputs a 1x3 matrix with the spherical coordinates of the drone realtive to the sensor
                    end
             end
            end

         fprintf('USlive'); %prints the 

            if rem(row, 2) == 0
                for pos = 1:-15/180:0
                    % in steps of 1 degree
                    writePosition(servo1, pos); % tell servo to go to position in variable 'pos'
                    pause(delay);
                    column = int32(pos * 180/15) + 1;
                    USlive(a, column) = readDistance(ultrasonicObj) / 2.54 * 100;
                    w=abs(USlive(a,column)-USbaseline1(a,column));
                    x = abs(USlive(a,column)-USbaseline2(a,column));
                    y= abs(USlive(a,column)-USbaseline3(a,column));
                    if w > (.4 .* USbaseline1(a,column) ) & x > (.4 .* USbaseline2(a,column) ) & y > (.4 .* USbaseline1(a,column) )
                        detection(3) = (row-1)*15; %phi 
                        detection(2) = (column-1)*15; %theta
                        detection(1) = USlive(row,column); %distance
                        detection=detection %outputs a 1x3 matrix with the spherical coordinates of the drone realtive to the sensor
                    end
                end
            end
    end
end
