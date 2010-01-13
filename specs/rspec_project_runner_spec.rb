require 'rspec_project_runner_spec_helper'

describe "Program Arguments Parser" do

	before :each do 
		@csproj = File.dirname(__FILE__) + '../rspec_project_runner/rspec_project_runner.csproj'
	end


	it "should parse -t as a target" do
		
		rargs = ['-t', @csproj]
		args = System::Array[System::String].new(rargs)
		
		p = Rspec::Project::Runner::ProgramArguments.Parse(args)
		p.T.should == args[1]
		
	end
	
	it "should parse -o as an output directory" do
	
		rargs = ['-o', 'C:\Users\pinvoke']
		args = System::Array[System::String].new(rargs)
	
		p = Rspec::Project::Runner::ProgramArguments.Parse(args)
		p.O.should == args[1]
		
	end
	
	it "should parse -r as a recursive project walk" do
	
		rargs = ['-r']
		args = System::Array[System::String].new(rargs)
	
		p = Rspec::Project::Runner::ProgramArguments.Parse(args)
		p.R.should == true
	
	end

	#todo: add tests for -?? and -? and -help
	
end

describe "PropertyGroup" do

	before :each do
		@basepath = 'C:\User'
		@propertyGroup = Rspec::Project::Runner::PropertyGroup.new(@basepath)	
	end
	
	it "should return a valid path when asked for full path" do
		
		@propertyGroup.AssemblyGroup = 'CmdParser'
		@propertyGroup.OutputType = 'Library'
		@propertyGroup.GetFullpath.should == @basepath + '\\bin\\Debug\\CmdParser.dll'
	
	end

end

describe "ProjectReference" do

	before :each do
		@proj_path = File.dirname(__FILE__) + '../rspec_project_runner/rspec_project_runner.csproj'
		@basepath = File.dirname(__FILE__) + '../rspec_project_runner/'
	end

	it "should return a .dll or .exe when asked for project file" do
		
		program_args = Rspec::Project::Runner::ProgramArguments.new(@proj_path, false, nil)
		projectReference = Rspec::Project::Runner::ProjectReference.new(program_args)
		projectReference.include = '..\cmd_parser\CmdParser.csproj'
		projectReference.name = 'CmdParser'
		finalPath = System::IO::Path.GetFullPath(@basepath + "..\\cmd_parser\\bin\\Debug\\CmdParser.dll")
		projectReference.GetProjectFullpath.should == finalPath
	end
	
	it "should build a new spec file when asked to render children"

end

describe "SpecBuilder" do

	it "should display all includes when called as to string"
	
	it "should render a complete rspec_spec file"

end

describe "Reference" do

	before :each do
		@basepath = File.dirname(__FILE__) + '../rspec_project_runner/'
		@reference = Rspec::Project::Runner::Reference.new(@basepath)
	end

	it "should return a full path to an assembly if local resource is called" do
	
		pending("Apparently I don't l know how to resolve files in Ruby yet...")
		@reference.hint_path = '..\\..\\test.dll'
		@reference.get_full_path.should == File.dirname(__FILE__) +  '..//..//test.dll' # fake dll, just for testing
		
	end
	
	it "should return a GAC reference to any reference to a GAC assembly" do
	
		@reference.HintPath = nil
		@reference.include = 'System.Xml.Linq'
		@reference.get_full_path.should == 'System.Xml.Linq, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
	
	end

end

#TODO:
# Add multiple .csproj as samples to run against
# refactor "render" method to use a RenderTemplateManager