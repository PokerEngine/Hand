project(eval)

add_executable(ps-recognize main.cpp)

target_link_libraries(ps-recognize
        peval
        penum
        ${Boost_LIBRARIES}
)
