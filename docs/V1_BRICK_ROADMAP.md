# VLA v1 Brick Implementation Roadmap

## Current State Analysis

### Existing Bricks (2 implemented)
- **Addition**: Basic arithmetic addition (i32 + i32 → i32)
- **Euclidean Division**: Division with quotient and remainder (i32 ÷ i32 → quotient, remainder)

### Current Architecture
- Compile-time brick generation using `brick!` macro
- Type-safe execution with BrickArgument, BrickInput, BrickOutput
- Static collection via `get_all_bricks()`
- Support for mixed parameter types (arguments, inputs, outputs)
- Tuple return support for multiple outputs

## v1 Core Brick Categories

Based on the extension system analysis and typical visual programming needs, here are the essential bricks for v1:

### 1. Mathematical Operations (Priority: High)
**Implementation Time: 1-2 weeks**

#### Basic Arithmetic
- [x] Addition (already implemented)
- [ ] **Subtraction** - `subtract(a: i32, b: i32) → i32`
- [ ] **Multiplication** - `multiply(a: i32, b: i32) → i32`
- [x] Euclidean Division (already implemented)
- [ ] **Modulo** - `modulo(a: i32, b: i32) → i32`
- [ ] **Power** - `power(base: i32, exponent: i32) → i32`

#### Advanced Math
- [ ] **Absolute Value** - `abs(value: i32) → i32`
- [ ] **Min/Max** - `min(a: i32, b: i32) → i32`, `max(a: i32, b: i32) → i32`
- [ ] **Square Root** - `sqrt(value: i32) → i32` (integer result)
- [ ] **Random Number** - `random_int(min: i32, max: i32) → i32`

### 2. String Operations (Priority: High)
**Implementation Time: 2-3 weeks**

#### Basic String Manipulation
- [ ] **Concatenate** - `concat(a: String, b: String) → String`
- [ ] **String Length** - `string_length(text: String) → i32`
- [ ] **Substring** - `substring(text: String, start: i32, length: i32) → String`
- [ ] **String Replace** - `replace(text: String, search: String, replacement: String) → String`
- [ ] **String Split** - `split(text: String, delimiter: String) → String` (returns first part for v1)

#### String Utilities
- [ ] **To Upper Case** - `to_upper(text: String) → String`
- [ ] **To Lower Case** - `to_lower(text: String) → String`
- [ ] **Trim Whitespace** - `trim(text: String) → String`
- [ ] **String Contains** - `contains(text: String, search: String) → bool`
- [ ] **String Starts With** - `starts_with(text: String, prefix: String) → bool`
- [ ] **String Ends With** - `ends_with(text: String, suffix: String) → bool`

### 3. Boolean Logic (Priority: High)
**Implementation Time: 1 week**

#### Logical Operations
- [ ] **AND** - `logical_and(a: bool, b: bool) → bool`
- [ ] **OR** - `logical_or(a: bool, b: bool) → bool`
- [ ] **NOT** - `logical_not(value: bool) → bool`
- [ ] **XOR** - `logical_xor(a: bool, b: bool) → bool`

#### Comparison Operations
- [ ] **Equal** - `equal(a: i32, b: i32) → bool`
- [ ] **Not Equal** - `not_equal(a: i32, b: i32) → bool`
- [ ] **Greater Than** - `greater_than(a: i32, b: i32) → bool`
- [ ] **Less Than** - `less_than(a: i32, b: i32) → bool`
- [ ] **Greater Than or Equal** - `gte(a: i32, b: i32) → bool`
- [ ] **Less Than or Equal** - `lte(a: i32, b: i32) → bool`

### 4. Type Conversion (Priority: Medium)
**Implementation Time: 1 week**

#### Number/String Conversion
- [ ] **Number to String** - `number_to_string(value: i32) → String`
- [ ] **String to Number** - `string_to_number(text: String) → i32` (with error handling)
- [ ] **Boolean to String** - `boolean_to_string(value: bool) → String`
- [ ] **String to Boolean** - `string_to_boolean(text: String) → bool`

### 5. Conditional Logic (Priority: Medium)
**Implementation Time: 1-2 weeks**

#### Basic Conditionals
- [ ] **If-Then-Else** - `if_then_else(condition: bool, if_true: String, if_false: String) → String`
- [ ] **Switch/Case** - `switch_case(value: String, case1: String, result1: String, default: String) → String`

### 6. Data Structures (Priority: Medium)
**Implementation Time: 2-3 weeks**

#### Array Operations (using String representation for v1)
- [ ] **Create Array** - `create_array(item1: String, item2: String, item3: String) → String` (JSON array)
- [ ] **Array Length** - `array_length(array: String) → i32`
- [ ] **Get Array Item** - `get_array_item(array: String, index: i32) → String`
- [ ] **Array Contains** - `array_contains(array: String, item: String) → bool`

#### Object Operations (using String representation for v1)
- [ ] **Create Object** - `create_object(key1: String, value1: String, key2: String, value2: String) → String` (JSON object)
- [ ] **Get Object Value** - `get_object_value(object: String, key: String) → String`
- [ ] **Set Object Value** - `set_object_value(object: String, key: String, value: String) → String`

### 7. Utility Functions (Priority: Low)
**Implementation Time: 1 week**

#### System Utilities
- [ ] **Current Timestamp** - `current_timestamp() → i32`
- [ ] **Sleep/Delay** - `delay(milliseconds: i32) → String` (returns "completed")
- [ ] **Debug Print** - `debug_print(message: String) → String` (returns message for chaining)

#### Format Utilities
- [ ] **Format String** - `format_string(template: String, value1: String, value2: String) → String`
- [ ] **JSON Parse** - `json_parse(json: String, path: String) → String`
- [ ] **JSON Stringify** - `json_stringify(value: String) → String`

## Implementation Strategy

### Phase 1: Core Operations (Weeks 1-3)
Focus on the most commonly needed bricks that enable basic workflows:

1. **Mathematical Operations** (Week 1)
   - Complete basic arithmetic: subtraction, multiplication, modulo, power
   - Add utility math: abs, min/max, sqrt

2. **String Operations** (Week 2)
   - Basic manipulation: concat, length, substring, replace
   - String utilities: upper/lower case, trim, contains

3. **Boolean Logic** (Week 3)
   - Logical operations: AND, OR, NOT, XOR
   - Comparison operations: all comparison operators

**Target: 35-40 bricks implemented**

### Phase 2: Advanced Features (Weeks 4-6)

1. **Type Conversion** (Week 4)
   - All number/string/boolean conversions

2. **Conditional Logic** (Week 5)
   - If-then-else, switch-case operations

3. **Data Structures** (Week 6)
   - Basic array and object operations using JSON strings

**Target: 55-65 bricks implemented**

### Phase 3: Utilities & Polish (Week 7)

1. **Utility Functions**
   - System utilities, format operations
   - JSON parsing utilities

2. **Testing & Documentation**
   - Comprehensive tests for all bricks
   - Usage examples and documentation

**Target: 70+ bricks implemented**

## Technical Implementation Notes

### Brick Macro Usage Examples

```rust
// Basic arithmetic
brick! {
    #[id("subtraction")]
    #[label("Subtraction")]
    #[description("Subtracts second number from first")]
    fn subtraction(
        #[input] #[label("Minuend")] a: i32,
        #[input] #[label("Subtrahend")] b: i32
    ) -> (
        #[label("Difference")] i32
    ) {
        (a - b,)
    }
}

// String operations with multiple inputs
brick! {
    #[id("string_replace")]
    #[label("String Replace")]
    #[description("Replaces all occurrences of search string with replacement")]
    fn string_replace(
        #[input] #[label("Text")] text: String,
        #[input] #[label("Search")] search: String,
        #[input] #[label("Replacement")] replacement: String
    ) -> (
        #[label("Result")] String
    ) {
        (text.replace(&search, &replacement),)
    }
}

// Conditional with argument
brick! {
    #[id("if_then_else")]
    #[label("If-Then-Else")]
    #[description("Returns one of two values based on condition")]
    fn if_then_else(
        #[input] #[label("Condition")] condition: bool,
        #[argument] #[label("If True")] if_true: String = "true",
        #[argument] #[label("If False")] if_false: String = "false"
    ) -> (
        #[label("Result")] String
    ) {
        (if condition { if_true } else { if_false },)
    }
}
```

### Error Handling Strategy

For v1, use safe defaults and avoid panics:

- **String parsing**: Return default values on parse errors
- **Array access**: Return empty string for out-of-bounds
- **Division by zero**: Return 0 or max value
- **JSON parsing**: Return empty string on invalid JSON

### Testing Requirements

Each brick must have:
- Unit tests for normal operation
- Edge case tests (empty strings, zero values, etc.)
- Error handling tests
- Documentation examples that work

## Success Metrics for v1

### Functionality Coverage
- **70+ bricks** covering core use cases
- **100% test coverage** for all implemented bricks
- **Zero panics** - all error cases handled gracefully

### User Experience
- **Clear labels** - each brick has intuitive naming
- **Good defaults** - sensible argument defaults where applicable
- **Consistent patterns** - similar operations work similarly

### Performance
- **<1ms execution** per brick for typical operations
- **Memory efficient** - no unnecessary allocations
- **Serialization support** - all bricks serialize correctly

## Future Extensions Beyond v1

After v1 is stable, consider:

### Advanced Data Types
- First-class Array and Object types (not JSON strings)
- Date/Time operations
- File I/O operations (read-only for security)

### Advanced Logic
- Loop constructs
- Advanced conditionals
- Pattern matching

### External Integration
- HTTP request bricks (with capability system)
- Database operations (read-only)
- Email sending (with permissions)

## Risk Mitigation

### Complexity Management
- **Start simple** - implement basic versions first
- **Consistent patterns** - use same patterns across similar bricks
- **Incremental testing** - test each brick as it's implemented

### Performance Concerns
- **Benchmark critical bricks** - especially string and array operations
- **Optimize hot paths** - profile actual usage patterns
- **Memory monitoring** - track allocation patterns

### Security Considerations
- **No file system access** in v1 bricks
- **No network access** in v1 bricks
- **Input validation** on all string operations
- **Safe parsing** with proper error handling

## Conclusion

This v1 roadmap provides a solid foundation of 70+ essential bricks that cover the most common visual programming use cases. The phased approach allows for iterative development and testing while building toward a comprehensive brick library.

The focus on mathematical operations, string manipulation, boolean logic, and basic data structures provides users with the tools needed to build meaningful workflows without the complexity of advanced extension systems.

After v1 proves successful, the extension system analysis provides a clear path forward for community contributions and advanced capabilities.