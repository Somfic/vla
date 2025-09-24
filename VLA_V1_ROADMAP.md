# VLA v1 Roadmap: Visual Programming Application

## Overview

This roadmap outlines the development plan to take VLA from its current basic canvas implementation to a fully functional v1 ready for beta testing. VLA is a visual programming application where users create logical scripts by connecting nodes (bricks) on a canvas.

**Current State**: Basic canvas with draggable nodes, connection capability, and 2 implemented bricks (Addition, Euclidean Division).

**Target State**: Complete visual programming environment with comprehensive brick library, execution engine, and polished user experience.

---

## Epic 1: Core Canvas Functionality (Highest Priority)

### 1.1 Brick Placement & Management
**Priority**: Critical
**Estimated Time**: 2-3 weeks

#### Issues to Create:
- **Brick Palette/Sidebar** - Create a searchable palette where users can select and drag bricks onto the canvas
  - Implement categories (Math, String, Logic, etc.)
  - Add search functionality
  - Drag-and-drop from palette to canvas
  - Show brick descriptions and previews

- **Node Selection & Deletion** - Allow users to select bricks on the canvas and delete them
  - Click to select individual bricks
  - Multiple selection with Ctrl+click or drag selection
  - Delete key or right-click context menu to remove bricks
  - Confirm deletion dialog for connected bricks

- **Canvas Context Menu** - Right-click menu for canvas operations
  - Add brick at cursor position
  - Paste brick
  - Canvas zoom/fit options

### 1.2 Connection Management
**Priority**: Critical
**Estimated Time**: 2-3 weeks

#### Issues to Create:
- **Connection Removal** - Allow users to disconnect edges between bricks
  - Click on edge to select and delete
  - Right-click on edge for deletion menu
  - Visual feedback for selected edges
  - Keyboard shortcut (Delete) for selected edges

- **Connection Validation** - Ensure type-safe connections between bricks
  - Prevent incompatible type connections (Number to String, etc.)
  - Visual indicators for valid/invalid connection attempts
  - Error messages for invalid connections
  - Auto-conversion where appropriate (Number to String)

- **Connection Visualization** - Improve visual representation of connections
  - Different line styles for different data types
  - Curved connections for better readability
  - Hover effects on connections
  - Connection labels showing data type

### 1.3 Default Values & Arguments
**Priority**: High
**Estimated Time**: 1-2 weeks

#### Issues to Create:
- **Default Input Values** - Set default values for unconnected inputs
  - Input fields appear on bricks for unconnected inputs
  - Type-appropriate input controls (number, text, boolean)
  - Save default values with graph
  - Visual distinction between connected and unconnected inputs

- **Argument Configuration** - Allow users to configure brick arguments
  - Inline argument editing on bricks
  - Expandable argument panels
  - Type validation for arguments
  - Reset to default functionality

---

## Epic 2: Expanded Brick Library (High Priority)

### 2.1 Mathematical Operations
**Priority**: High
**Estimated Time**: 2 weeks

#### Issues to Create:
- **Basic Arithmetic Bricks** - Complete the fundamental math operations
  - Subtraction (a - b → result)
  - Multiplication (a × b → result)
  - Division (a ÷ b → result)
  - Modulo (a % b → remainder)
  - Power (base ^ exponent → result)

- **Advanced Math Bricks** - Add utility mathematical functions
  - Absolute Value (|a| → result)
  - Min/Max (a, b → min/max)
  - Square Root (√a → result)
  - Round/Floor/Ceiling (a → result)
  - Random Number (min, max → random)

### 2.2 String Operations
**Priority**: High
**Estimated Time**: 2-3 weeks

#### Issues to Create:
- **Basic String Manipulation** - Essential string operations
  - Concatenation (a + b → result)
  - String Length (text → length)
  - Substring (text, start, length → substring)
  - String Replace (text, search, replace → result)
  - String Split (text, delimiter → parts) *simplified for v1*

- **String Utilities** - Common string transformations
  - To Upper Case (text → UPPER)
  - To Lower Case (text → lower)
  - Trim Whitespace (text → trimmed)
  - String Contains (text, search → boolean)
  - Starts With / Ends With (text, prefix/suffix → boolean)

### 2.3 Boolean Logic & Comparison
**Priority**: High
**Estimated Time**: 1-2 weeks

#### Issues to Create:
- **Logical Operations** - Boolean logic bricks
  - AND (a && b → result)
  - OR (a || b → result)
  - NOT (!a → result)
  - XOR (a ⊕ b → result)

- **Comparison Operations** - Numeric and string comparisons
  - Equal (a == b → boolean)
  - Not Equal (a != b → boolean)
  - Greater Than (a > b → boolean)
  - Less Than (a < b → boolean)
  - Greater/Less Than or Equal (a >= b, a <= b → boolean)

### 2.4 Type Conversion & Conditionals
**Priority**: Medium
**Estimated Time**: 1-2 weeks

#### Issues to Create:
- **Type Conversion Bricks** - Convert between data types
  - Number to String (123 → "123")
  - String to Number ("123" → 123)
  - Boolean to String (true → "true")
  - String to Boolean ("true" → true)

- **Conditional Logic Bricks** - Basic conditional operations
  - If-Then-Else (condition, ifTrue, ifFalse → result)
  - Switch/Case (value, cases → result) *simplified for v1*

---

## Epic 3: Execution Engine (Medium-High Priority)

### 3.1 Basic Execution System
**Priority**: Medium-High
**Estimated Time**: 3-4 weeks

#### Issues to Create:
- **Graph Execution Engine** - Core execution system
  - Topological sort for execution order
  - Dependency resolution between connected bricks
  - Data flow execution (values flow through connections)
  - Error handling and execution termination

- **Execute Button & Results** - User interface for execution
  - Execute button on canvas
  - Execution progress indicator
  - Results display panel
  - Error message display
  - Clear results functionality

- **Node Output Visualization** - Show execution results on nodes
  - Display output values on brick output handles
  - Visual indicators for successful/failed execution
  - Hover tooltips showing intermediate results
  - Execution trace/debugging information

### 3.2 Advanced Execution Features
**Priority**: Medium
**Estimated Time**: 2-3 weeks

#### Issues to Create:
- **Step-by-Step Execution** - Debug mode execution
  - Step button to execute one brick at a time
  - Highlight currently executing brick
  - Pause/resume execution
  - Execution state visualization

- **Execution History** - Track execution results
  - Save execution results with timestamps
  - Compare different execution runs
  - Export execution results
  - Performance metrics (execution time, brick count)

---

## Epic 4: User Experience & Polish (Medium Priority)

### 4.1 Canvas Enhancements
**Priority**: Medium
**Estimated Time**: 2-3 weeks

#### Issues to Create:
- **Improved Node Layout** - Better visual design for bricks
  - Consistent spacing and sizing
  - Clear input/output handle positioning
  - Improved typography and colors
  - Icons for different brick types

- **Canvas Navigation** - Better canvas interaction
  - Smooth zoom in/out with mouse wheel
  - Pan with middle mouse or space+drag
  - Fit to window functionality
  - Mini-map for large graphs

- **Grid & Snapping** - Alignment helpers
  - Optional grid overlay
  - Snap-to-grid when moving nodes
  - Smart guides for alignment
  - Consistent spacing helpers

### 4.2 Graph Management
**Priority**: Medium
**Estimated Time**: 2 weeks

#### Issues to Create:
- **Save/Load Improvements** - Better file management
  - Save As functionality with file dialog
  - Recent files menu
  - Auto-save with configurable intervals
  - File format versioning

- **Graph Validation** - Ensure graph correctness
  - Detect cycles in connections
  - Validate all connections are type-safe
  - Warning for unconnected required inputs
  - Graph statistics (node count, connection count)

### 4.3 Keyboard Shortcuts & Accessibility
**Priority**: Medium
**Estimated Time**: 1-2 weeks

#### Issues to Create:
- **Keyboard Shortcuts** - Common operations with keyboard
  - Ctrl+S for save
  - Delete for selected nodes/edges
  - Ctrl+Z/Y for undo/redo
  - Ctrl+A for select all
  - F5 for execute

- **Undo/Redo System** - Action history management
  - Track all user actions (add, delete, move, connect)
  - Undo/redo with keyboard shortcuts
  - Action history panel
  - Limit undo history to prevent memory issues

---

## Epic 5: Data Structures & Advanced Features (Lower Priority)

### 5.1 Basic Data Structures
**Priority**: Low-Medium
**Estimated Time**: 3-4 weeks

#### Issues to Create:
- **Array Operations** - Basic array/list handling (using JSON strings for v1)
  - Create Array (item1, item2, item3 → JSON array)
  - Get Array Item (array, index → item)
  - Array Length (array → count)
  - Array Contains (array, item → boolean)

- **Object/Map Operations** - Key-value pair handling
  - Create Object (key1, value1, key2, value2 → JSON object)
  - Get Object Value (object, key → value)
  - Set Object Value (object, key, value → new object)
  - Object Keys (object → keys array)

### 5.2 Utility Functions
**Priority**: Low
**Estimated Time**: 1-2 weeks

#### Issues to Create:
- **System Utilities** - System interaction bricks
  - Current Timestamp (→ timestamp)
  - Delay/Sleep (milliseconds → completed)
  - Debug Print (message → message) *for development*
  - Generate UUID (→ unique ID)

- **Format & Parse Utilities** - Data formatting
  - Format String (template, values → formatted)
  - JSON Parse (json string, path → value)
  - JSON Stringify (value → json string)
  - Number Formatting (number, decimals → formatted)

---

## Epic 6: Testing & Documentation (Ongoing)

### 6.1 Testing Infrastructure
**Priority**: Medium
**Estimated Time**: 2-3 weeks

#### Issues to Create:
- **Automated Testing** - Comprehensive test suite
  - Unit tests for all brick functions
  - Integration tests for execution engine
  - UI tests for canvas interactions
  - Performance benchmarks

- **Example Graphs** - Demonstrate functionality
  - Tutorial graphs for new users
  - Complex example workflows
  - Performance test graphs
  - Edge case demonstration graphs

### 6.2 Documentation & Help
**Priority**: Medium
**Estimated Time**: 1-2 weeks

#### Issues to Create:
- **In-App Help** - User guidance within the application
  - Tooltip help for all UI elements
  - Interactive tutorial/onboarding
  - Help panel with keyboard shortcuts
  - Context-sensitive help

- **Brick Documentation** - Complete brick reference
  - Auto-generated brick documentation
  - Usage examples for each brick
  - Best practices guide
  - Common patterns and workflows

---

## Implementation Timeline

### Phase 1: Core Functionality (Weeks 1-8)
**Focus**: Make the canvas fully functional for basic use
- Complete Epic 1 (Canvas functionality)
- Start Epic 2 (Math and String bricks)
- Basic execution from Epic 3

**Milestone**: Users can create, connect, and execute simple graphs

### Phase 2: Comprehensive Bricks (Weeks 9-14)
**Focus**: Complete the essential brick library
- Finish Epic 2 (All basic brick types)
- Complete Epic 3 (Full execution engine)
- Start Epic 4 (UX improvements)

**Milestone**: All essential programming constructs available

### Phase 3: Polish & Advanced Features (Weeks 15-20)
**Focus**: Prepare for beta testing
- Complete Epic 4 (Polish and UX)
- Epic 5 (Advanced features if time allows)
- Epic 6 (Testing and documentation)

**Milestone**: Ready for beta testing with real users

---

## Success Criteria for v1

### Functional Requirements
- ✅ **70+ bricks** covering core programming constructs
- ✅ **Complete canvas functionality** - place, connect, delete, configure
- ✅ **Execution engine** - correct execution of connected graphs
- ✅ **Save/load** - persistent storage of graphs
- ✅ **Type safety** - prevent invalid connections

### User Experience Requirements
- ✅ **Intuitive interface** - new users can create graphs within 5 minutes
- ✅ **Responsive performance** - smooth interactions even with large graphs
- ✅ **Clear visual feedback** - users understand system state at all times
- ✅ **Error recovery** - graceful handling of all error conditions

### Technical Requirements
- ✅ **Cross-platform** - works on Windows, macOS, Linux
- ✅ **Performance** - handles graphs with 100+ nodes smoothly
- ✅ **Stability** - no crashes or data loss under normal usage
- ✅ **Extensibility** - architecture ready for future enhancements

---

## Risk Mitigation

### Technical Risks
- **Complexity management**: Start simple, add features incrementally
- **Performance issues**: Profile early, optimize bottlenecks
- **Type system edge cases**: Comprehensive testing of all combinations

### User Experience Risks
- **Steep learning curve**: Provide excellent onboarding and tutorials
- **Overwhelming interface**: Progressive disclosure, hide advanced features initially
- **Unclear visual language**: Consistent design system, user testing

### Schedule Risks
- **Feature scope creep**: Strict prioritization, defer non-essential features
- **Technical blockers**: Parallel development tracks where possible
- **Quality assurance time**: Build testing into development process

---

## Post-v1 Considerations

After successful v1 launch and beta feedback:

### Potential v1.1 Features
- **Execution flow control** (from EXECUTION_FLOW_IMPLEMENTATION_PLAN.md)
- **Plugin system** (from COMPREHENSIVE_EXTENSION_SYSTEM.md)
- **Advanced data structures** (first-class arrays/objects)
- **File I/O operations**
- **Network requests** (with security model)

### Community & Ecosystem
- **Brick marketplace** - sharing custom bricks
- **Templates gallery** - common workflow templates
- **Export options** - generate code from graphs
- **Integration APIs** - embed VLA in other applications

---

This roadmap provides a clear path from the current basic implementation to a feature-complete v1 ready for user testing. The epic-based organization makes it easy to track progress and adjust priorities based on user feedback during development.