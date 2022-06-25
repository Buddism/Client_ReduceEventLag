// function wrenchEventsDlg::CreateColorMenu (%this, %gui)
// {
// 	%rowLimit = -1;
// 	for(%i = 0; %i < $Paint_NumPaintRows; %i++)
// 		%rowLimit = getMax(%rowLimit, $Paint_Row[%i].numSwatches);

// 	%xPos = getWord (%gui.getPosition(), 0) + getWord (%gui.getExtent(), 0);
// 	%yPos = getWord (%gui.getPosition(), 1);
// 	%parent = %gui;

// 	for (%i = 0; %i < 3; %i++)
// 	{
// 		%parent = %parent.getGroup ();
// 		%xPos += getWord (%parent.getPosition (), 0);
// 		%yPos += getWord (%parent.getPosition (), 1);
// 	}

// 	if (isObject (%this.colorMenu))
// 	{
// 		%oldx = getWord (%this.colorMenu.getPosition (), 0);
// 		%oldy = getWord (%this.colorMenu.getPosition (), 1);
// 		%this.colorMenu.delete ();
// 		if (%oldx == %xPos && %oldy == %yPos)
// 			return;
// 	}
// 	%newScroll = new GuiScrollCtrl("Avatar_ColorMenu")
// 	{
// 		Profile = ColorScrollProfile;
// 		vScrollBar = "alwaysOn";
// 		hScrollBar = "alwaysOff";

// 		position = %xPos SPC %yPos;
// 		extent = 18+12 SPC 18;
// 	};
// 	WrenchEvents_Window.add (%newScroll);

// 	%newBox = new GuiSwatchCtrl()
// 	{
// 		color = "0 0 0 1";
// 		position = "0 0";
// 		extent = "18 18";
// 	};
// 	%newScroll.add(%newBox);

// 	%itemCount = 0;
// 	%numPaintRows = 0;
// 	%lastPaintRowMax = $Paint_Row[%numPaintRows].numSwatches;
// 	for(%color = getColorIDTable(%i = 0); getWord(%color, 3) > 0 && %i < 64; %color = getColorIDTable(%i++))
// 	{
// 		if(%i >= %lastPaintRowMax)
// 		{
// 			echo("new row: " @ %i);
// 			%numPaintRows++;
// 			%lastPaintRowMax = %i + $Paint_Row[%numPaintRows].numSwatches;
// 		}
// 		if (%color $= "")
// 			%color = "1 1 1 1";

// 		//%rowLimit = $Paint_Row[%numPaintRows].numSwatches;

// 		%x = (%itemCount % %rowLimit) * 18;
// 		%y = mFloor (%itemCount / %rowLimit) * 18;

// 		%newSwatch = new GuiSwatchCtrl()
// 		{
// 			position = %x SPC %y;
// 			extent = "18 18";
// 		};
// 		%newSwatch.setColor(%color);

// 		%newButton = new GuiBitmapButtonCtrl()
// 		{
// 			bitmap = "base/client/ui/btnColor";
// 			position = %x SPC %y;
// 			extent = "18 18";
// 			command = "wrenchEventsDlg.pickColor(" @ %gui @ "," @ %i @ ");";
// 			text = " ";
// 		};
		
// 		%newBox.add (%newSwatch);
// 		%newBox.add (%newButton);

// 		%itemCount++;
// 	}
// 	if (%itemCount >= %rowLimit)
// 		%w = %rowLimit * 18;
// 	else
// 		%w = %itemCount * 18;

// 	%h = (mFloor (%itemCount / %rowLimit) + 0) * 18;
// 	%newBox.resize (0, 0, %w, %h);
// 	if (%yPos + %h > 480)
// 		%h = mFloor ((480 - %yPos) / 18) * 18;

// 	%newScroll.resize (%xPos, %yPos, %w + 12, %h);
// 	%this.colorMenu = %newScroll;
// }

package doot
{
	function ClientCmdRegisterEventsDone()
	{
		$generatedSoundList = 0;
		$generatedMusicList = 0;
		$generatedVehicleList = 0;
		deleteVariables("$Gen::ML*");
		deleteVariables("$Gen::SL*");
		deleteVariables("$Gen::VL*");
		deleteVariables("$generatedCustom*");
		deleteVariables("$Gen::Custom*");
		newChatHud_AddLine(request);
		parent::ClientCmdRegisterEventsDone();
	}
	function wrenchEventsDlg::createOutputParameters(%this, %box, %outputMenu, %outputClass)
	{
		//newChatHud_AddLine(hi);
		%this.closeColorMenu();
		%lastObject = %box.getObject(%box.getCount() - 1.0);
		while(%lastObject != %outputMenu)
		{
			%lastObject.delete();
			%lastObject = %box.getObject(%box.getCount() - 1.0);
		}
		if(%outputMenu.getText() $= "")
			return;

		%selected = %outputMenu.getSelected();
		%parList = $OutputEvent_parameterList[%outputClass,%selected];
		%focusControl = 0;
		%parCount = getFieldCount(%parList);

		for(%i = 0; %i < %parCount; %i++)
		{
			%field = getField(%parList, %i);
			%lastObject = %box.getObject(%box.getCount() - 1.0);
			%x = 2.0 + getWord(%lastObject.getPosition(), 0) + getWord(%lastObject.getExtent(), 0);
			%y = 0;
			%h = 18;
			%type = getWord(%field, 0);
			switch$(%type)
			{
				case "int":
					%min = mFloor(getWord(%field, 1));
					%max = mFloor(getWord(%field, 2));
					%default = mFloor(getWord(%field, 3));
					%maxChars = 1;
					if(%min < 0.0)
						%testVal = getMax(mAbs(%min) * 10.0, %max);
					else %testVal = getMax(mAbs(%min), %max);
					while(%testVal >= 10.0)
					{
						%maxChars = %maxChars + 1.0;
						%testVal = %testVal / 10.0;
					}
					%gui = new GuiTextEditCtrl();
					%box.add(%gui);
					%w = %maxChars * 6.0 + 6.0;
					%gui.resize(%x, %y, %w, %h);
					%gui.command = "wrenchEventsDlg.VerifyInt(" @ %gui @ "," @ %min @ "," @ %max @ ");";
					%gui.setText(%default);
				case "intList":
					%maxLength = 200;
					%width = mFloor(getWord(%field, 1));
					%gui = new GuiTextEditCtrl();
					%box.add(%gui);
					%w = %width;
					%gui.resize(%x, %y, %w, %h);
					%gui.maxLength = %maxLength;
				case "float":
						%min = atof(getWord(%field, 1));
						%max = atof(getWord(%field, 2));
						%step = mAbs(getWord(%field, 3));
						%default = atof(getWord(%field, 4));
						if (%step >= %max - %min)
							%step = (%max - %min) / 10.0;
						if (%step <= 0.0)
							%step = 0.1;
						%gui = new GuiSliderCtrl();
						%box.add(%gui);
						%w = 100;
						%h = 36;
						%gui.resize(%x, %y, %w, %h);
						%gui.range = %min SPC %max;
						%gui.setValue(%default);
						%gui.command = " $thisControl.setValue(       mFloor( $thisControl.getValue() * (1 / " @ %step @ ") )   * " @ %step @ "   ) ;";
				case "bool":
						%gui = new GuiCheckBoxCtrl();
						%box.add(%gui);
						%w = %h;
						%gui.resize(%x, %y, %w, %h);
						%gui.setText("");
				case "string":
						%maxLength = mFloor(getWord(%field, 1));
						%width = mFloor(getWord(%field, 2));
						%gui = new GuiTextEditCtrl();
						%box.add(%gui);
						%w = %width;
						
						%gui.resize(%x, %y, %w, %h);
						%gui.maxLength = %maxLength;
				case "datablock":
						%dbClassName = getWord(%field, 1);
						%gui = new GuiPopUpMenuCtrl();
						%box.add(%gui);
						%w = 100;
						%gui.resize(%x, %y, %w, %h);
						%dbCount = mClamp(getDataBlockGroupSize(), 0, 100000);
						switch$(%dbClassName)
						{
							case "Music":
								if(!$generatedMusicList)
								{
									for(%itr = 0; %itr < %dbCount; %itr++)
									{
										%db = getDataBlock(%itr);
										%dbClass = %db.getClassName();
										if (%dbClass $= "AudioProfile")
											if (%db.uiName !$= "")
											{
												%gui.add(%db.uiName, %db);
												$Gen::MLN[-1+$Gen::MLNC++] = %db.uiName;
												$Gen::MLD[-1+$Gen::MLDC++] = %db;
											}
									}
									$generatedMusicList = 1;
								} else {
									%gcount = getMax($Gen::MLNC, $Gen::MLDC);
									for(%sI = 0; %sI < %gcount; %sI++)
										%gui.add($Gen::MLN[%sI], $gen::MLD[%sI]);
								}
							case "Sound":
								if(!$generatedSoundList)
								{
									for(%itr = 0; %itr < %dbCount; %itr++)
									{
										%db = getDataBlock(%itr);
										%dbClass = %db.getClassName();
										if (%dbClass $= "AudioProfile")
										{
											if (%db.uiName $= "" && %db.getDescription().isLooping != 1 && %db.getDescription().is3D)
											{
												%name = fileName(%db.fileName);
												%gui.add(%name, %db);
												$Gen::SLN[-1+$Gen::SLNC++] = %name;
												$Gen::SLD[-1+$Gen::SLDC++] = %db;
											}
										}
									}
									$generatedSoundList = 1;
								} else {
									%gcount = getMax($Gen::SLNC, $Gen::SLDC);
									for(%sI = 0; %sI < %gcount; %sI++)
										%gui.add($Gen::SLN[%sI], $gen::SLD[%sI]);
								}
							case "Vehicle":
								if(!$generatedVehicleList)
								{
									for(%itr = 0; %itr < %dbCount; %itr++)
									{
										%db = getDataBlock(%itr);
										%dbClass = %db.getClassName();
										if (%db.uiName !$= "")
											if (%dbClass $= "WheeledVehicleData" || %dbClass $= "HoverVehicleData" || %dbClass $= "FlyingVehicleData" || (%dbClass $= "PlayerData" && %db.rideAble))
											{
												%gui.add(%db.uiName, %db);
												$Gen::VLN[-1+$Gen::VLNC++] = %db.uiName;
												$Gen::VLD[-1+$Gen::VLDC++] = %db;
											}
									}
									$generatedVehicleList = 1;
								} else {
									%gcount = getMax($Gen::VLNC, $Gen::VLDC);
									for(%sI = 0; %sI < %gcount; %sI++)
										%gui.add($Gen::VLN[%sI], $gen::VLD[%sI]);
								}
							default:
								if(!$generatedCustom[%dbClassName])
								{
									for(%itr = 0; %itr < %dbCount; %itr++)
									{
										%db = getDataBlock(%itr);
										%dbClass = %db.getClassName();
										if (%db.uiName !$= "" && %dbClass $= %dbClassName)
										{
											%gui.add(%db.uiName, %db);
											$Gen::CustomN[%dbClassName, -1 + $Gen::CustomCN[%dbClassName]++] = %db.uiName;
											$Gen::CustomD[%dbClassName, -1 + $Gen::CustomCD[%dbClassName]++] = %db;
										}
									}
									$generatedCustom[%dbClassName] = 1;
								} else {
									%gcount = getMax($Gen::CustomCN[%dbClassName], $Gen::CustomCD[%dbClassName]);
									for(%sI = 0; %sI < %gcount; %sI++)
										%gui.add($Gen::CustomN[%dbClassName, %sI], $Gen::CustomD[%dbClassName, %sI]);
								}
						}
						%gui.sort();
						%gui.addFront("NONE", -1);
						if (!$WrenchEventLoading)
							%gui.forceOnAction();
				case "Vector":
					%tw = 31;
					%gui = new GuiSwatchCtrl();
					%box.add(%gui);
					%w = (%tw + 2.0) * 3.0 + 2.0;
					%gui.resize(%x, %y, %w, %h);
					%gui.setColor("0 0 0 0.75");
					%xTextBox = new GuiTextEditCtrl();
					%gui.add(%xTextBox);
					%tx = 0.0 + 2.0;
					%ty = 0;
					%th = %h;
					%xTextBox.resize(%tx, %ty, %tw, %th);
					%yTextBox = new GuiTextEditCtrl();
					%gui.add(%yTextBox);
					%tx = (%tw + 2.0) * 1.0 + 2.0;
					%yTextBox.resize(%tx, %ty, %tw, %th);
					%zTextBox = new GuiTextEditCtrl();
					%gui.add(%zTextBox);
					%tx = (%tw + 2.0) * 2.0 + 2.0;
					%zTextBox.resize(%tx, %ty, %tw, %th);
					%gui = %xTextBox;
				case "list":
					%gui = new GuiPopUpMenuCtrl();
					%box.add(%gui);
					%w = 100;
					%h = 18;
					%gui.resize(%x, %y, %w, %h);
					%itemCount = (getWordCount(%field) - 1.0) / 2.0;
					%itr = 0;
					while(%itr < %itemCount)
					{
						%idx = %itr * 2.0 + 1.0;
						%name = getWord(%field, %idx);
						%id = getWord(%field, %idx + 1.0);
						%gui.add(%name, %id);
						%itr = %itr + 1.0;
					}
					%gui.setSelected(0);
					if (!$WrenchEventLoading)
						%gui.forceOnAction();
				case "paintColor":
					%gui = new GuiSwatchCtrl();
					%box.add(%gui);
					%w = 18;
					%h = 18;
					%gui.resize(%x, %y, %w, %h);
					%button = new GuiBitmapButtonCtrl();
					%gui.add(%button);
					%button.resize(0, 0, %w, %h);
					%button.setBitmap("base/client/ui/btnColor");
					%button.setText("");
					%button.command = "WrenchEventsDlg.CreateColorMenu(" @ %gui @ ");";
					wrenchEventsDlg.pickColor(%gui, 0);
				default:
					error("ERROR: wrenchEventsDlg::createOutputParameters() - unknown type " @ %type @ "");
			}
			if (!%focusControl)
				%focusControl = %gui;
		}
		if(isObject(%focusControl))
		{
			%focusControl.makeFirstResponder(1);
		}
	}
};
activatePackage(doot);




//MEGA support
function populateButton(%button, %datablock)
{
	//if($EventSystem::Version != 2)
		//return parent::populateButton(%button,%dataBlock);
	%button.dataType = %datablock;
	%button.clear();
	if(%datablock $= "Music")
	{
		%datablock = "AudioProfile";
		for(%a = 0; %a < $ExtUINameTableCount[%datablock]; %a++)
			if($ExtUINameTableID[%datablock, %a] !$= "")
				%button.add($ExtUINameTableID[%datablock, %a].uiName, $ExtUINameTableID[%datablock, %a]);
	}
	else if(%datablock $= "Sound")
	{
		if(!$generatedSoundList)
		{
			%dbCount = getDataBlockGroupSize();
			for(%itr = 0; %itr < %dbCount; %itr++)
			{
				%db = getDataBlock(%itr);
				%dbClass = %db.getClassName();
				if (%dbClass $= "AudioProfile")
				{
					if (%db.uiName $= "" && %db.getDescription().isLooping != 1 && %db.getDescription().is3D)
					{
						%name = fileName(%db.fileName);
						%button.add(%name, %db);
						$EC::SLN[-1+$EC::SNC++] = %name;
						$EC::SLD[-1+$EC::SDC++] = %db;
					}
				}
			}
			$generatedSoundList = 1;
		} else {
			%gcount = getMax($EC::SNC, $EC::SDC);
			for(%sI = 0; %sI < %gcount; %sI++)
				%button.add($EC::SLN[%sI], $EC::SLD[%sI]);
		}
	}
	else if(%datablock $= "Vehicle")
	{
		if(!$generatedVehicleList)
		{
			%dbCount = getDataBlockGroupSize();
			for(%itr = 0; %itr < %dbCount; %itr++)
			{
				%db = getDataBlock(%itr);
				%dbClass = %db.getClassName();
				if (%db.uiName !$= "")
					if (%dbClass $= "WheeledVehicleData" || %dbClass $= "HoverVehicleData" || %dbClass $= "FlyingVehicleData" || (%dbClass $= "PlayerData" && %db.rideAble))
					{
						%button.add(%db.uiName, %db);
						$EC::VLN[-1+$EC::VLNC++] = %db.uiName;
						$EC::VLD[-1+$EC::VLDC++] = %db;
					}
			}
			$generatedVehicleList = 1;
		} else {
			%gcount = getMax($EC::VLNC, $EC::VLDC);
			for(%sI = 0; %sI < %gcount; %sI++)
				%button.add($EC::VLN[%sI], $EC::VLD[%sI]);
		}
	}
	else
	{
		for(%a = 0; %a < $ExtUINameTableCount[%datablock]; %a++)
		{
			if($ExtUINameTableID[%datablock, %a].uiName !$= "")
			{
				%button.add($ExtUINameTableID[%datablock, %a].uiName,$ExtUINameTableID[%datablock, %a]);
				%button.entryID[$ExtUINameTableID[%datablock, %a].uiName] = $ExtUINameTableID[%datablock, %a];
			}
		}
	}
	%button.sort();
	%button.addFront("None",-1);
	%button.entryID["None"] = -1;
}



//broken
return;
package doot
{
	function ClientCmdRegisterEventsDone()
	{
		$generatedSoundList = 0;
		$generatedMusicList = 0;
		$generatedVehicleList = 0;
		deleteVariables("$Gen::ML*");
		deleteVariables("$Gen::SL*");
		deleteVariables("$Gen::VL*");
		deleteVariables("$generatedCustom*");
		deleteVariables("$Gen::Custom*");
		newChatHud_AddLine(request);
		parent::ClientCmdRegisterEventsDone();
	}
	function wrenchEventsDlg::createOutputParameters(%this, %box, %outputMenu, %outputClass)
	{
		if($EventSystem::Version > 0)
			return parent::createOutputParameters(%this, %box, %outputMenu, %outputClass);

		%this.closeColorMenu();
		%lastObject = %box.getObject(%box.getCount() - 1.0);
		while(%lastObject != %outputMenu)
		{
			%lastObject.delete();
			%lastObject = %box.getObject(%box.getCount() - 1.0);
		}
		if(%outputMenu.getText() $= "")
			return;

		%selected = %outputMenu.getSelected();
		%parList = $OutputEvent_parameterList[%outputClass,%selected];
		%focusControl = 0;
		%parCount = getFieldCount(%parList);

		for(%i = 0; %i < %parCount; %i++)
		{
			%field = getField(%parList, %i);
			%lastObject = %box.getObject(%box.getCount() - 1.0);
			%x = 2.0 + getWord(%lastObject.getPosition(), 0) + getWord(%lastObject.getExtent(), 0);
			%y = 0;
			%h = 18;
			%type = getWord(%field, 0);
			switch$(%type)
			{
				case "datablock":
						%dbClassName = getWord(%field, 1);
						%gui = new GuiPopUpMenuCtrl();
						%box.add(%gui);
						%w = 100;
						%gui.resize(%x, %y, %w, %h);
						%dbCount = getDataBlockGroupSize();

						switch$(%dbClassName)
						{
							case "Music":
								if(!$generatedMusicList)
								{
									for(%itr = 0; %itr < %dbCount; %itr++)
									{
										%db = getDataBlock(%itr);
										%dbClass = %db.getClassName();
										if(%dbClass $= "fxDTSBrick")
											break; //no longer datablocks

										if (%dbClass $= "AudioProfile")
											if (%db.uiName !$= "")
											{
												%gui.add(%db.uiName, %db);
												$Gen::MLN[-1+$Gen::MLNC++] = %db.uiName;
												$Gen::MLD[-1+$Gen::MLDC++] = %db;
											}
									}
									$generatedMusicList = 1;
								} else {
									%gcount = getMax($Gen::MLNC, $Gen::MLDC);
									for(%sI = 0; %sI < %gcount; %sI++)
										%gui.add($Gen::MLN[%sI], $gen::MLD[%sI]);
								}
							case "Sound":
								if(!$generatedSoundList)
								{
									for(%itr = 0; %itr < %dbCount; %itr++)
									{
										%db = getDataBlock(%itr);
										%dbClass = %db.getClassName();
										if(%dbClass $= "fxDTSBrick")
										{
											break; //no longer datablocks
										}
										if (%dbClass $= "AudioProfile")
										{
											if (%db.uiName $= "" && %db.getDescription().isLooping != 1 && %db.getDescription().is3D)
											{
												%name = fileName(%db.fileName);
												%gui.add(%name, %db);
												$Gen::SLN[-1+$Gen::SLNC++] = %name;
												$Gen::SLD[-1+$Gen::SLDC++] = %db;
											}
										}
									}
									$generatedSoundList = 1;
								} else {
									%t = getRealTime();

									%gcount = getMax($Gen::SLNC, $Gen::SLDC);
									for(%sI = 0; %sI < %gcount; %sI++)
										%gui.add($Gen::SLN[%sI], $gen::SLD[%sI]);

									echo(getRealTime() - %t);
								}
							case "Vehicle":
								if(!$generatedVehicleList)
								{
									for(%itr = 0; %itr < %dbCount; %itr++)
									{
										%db = getDataBlock(%itr);
										%dbClass = %db.getClassName();
										if(%dbClass $= "fxDTSBrick")
											break; //no longer datablocks

										if (%db.uiName !$= "")
											if (%dbClass $= "WheeledVehicleData" || %dbClass $= "HoverVehicleData" || %dbClass $= "FlyingVehicleData" || (%dbClass $= "PlayerData" && %db.rideAble))
											{
												%gui.add(%db.uiName, %db);
												$Gen::VLN[-1+$Gen::VLNC++] = %db.uiName;
												$Gen::VLD[-1+$Gen::VLDC++] = %db;
											}
									}
									$generatedVehicleList = 1;
								} else {
									%gcount = getMax($Gen::VLNC, $Gen::VLDC);
									for(%sI = 0; %sI < %gcount; %sI++)
										%gui.add($Gen::VLN[%sI], $gen::VLD[%sI]);
								}
							default:
								if(!$generatedCustom[%dbClassName])
								{
									for(%itr = 0; %itr < %dbCount; %itr++)
									{
										%db = getDataBlock(%itr);
										%dbClass = %db.getClassName();
										if(%dbClass $= "fxDTSBrick")
											break; //no longer datablocks

										if (%db.uiName !$= "" && %dbClass $= %dbClassName)
										{
											%gui.add(%db.uiName, %db);
											$Gen::CustomN[%dbClassName, -1 + $Gen::CustomCN[%dbClassName]++] = %db.uiName;
											$Gen::CustomD[%dbClassName, -1 + $Gen::CustomCD[%dbClassName]++] = %db;
										}
									}
									$generatedCustom[%dbClassName] = 1;
								} else {
									%gcount = getMax($Gen::CustomCN[%dbClassName], $Gen::CustomCD[%dbClassName]);
									for(%sI = 0; %sI < %gcount; %sI++)
										%gui.add($Gen::CustomN[%dbClassName, %sI], $Gen::CustomD[%dbClassName, %sI]);
								}
						}
						%t = getRealTime();

						%gui.sort();
						%gui.addFront("NONE", -1);
						if (!$WrenchEventLoading)
							%gui.forceOnAction();

						echo(getRealTime() - %t);

				default:
					//error("ERROR: wrenchEventsDlg::createOutputParameters() - unknown type " @ %type @ "");

					//		TODO: FIX ERRORS
					return parent::createOutputParameters(%this, %box, %outputMenu, %outputClass);
			}
			if (!%focusControl)
				%focusControl = %gui;
		}
		if(isObject(%focusControl))
			%focusControl.makeFirstResponder(1);
	}
};
activatePackage(doot);
