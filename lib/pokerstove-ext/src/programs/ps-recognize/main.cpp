#include <boost/program_options.hpp>
#include <iostream>
#include <pokerstove/peval/PokerHandEvaluator.h>

using namespace pokerstove;
namespace po = boost::program_options;
using namespace std;

int main(int argc, char** argv)
{
    po::options_description desc("ps-recognize, a poker hand recognizer\n");

    desc.add_options()
        ("help,?",  "produce help message")
        ("game,g",  po::value<string>()->default_value("h"),    "game to use for recognition")
        ("board,b", po::value<string>(),                        "community cards for he/o/o8")
        ("hand,h",  po::value<string>(),                        "a hand for recognition");

    // make hand a positional argument
    po::positional_options_description p;
    p.add("hand", -1);

    po::variables_map vm;
    po::store(po::command_line_parser(argc, argv)
                  .style(po::command_line_style::unix_style)
                  .options(desc)
                  .positional(p)
                  .run(),
              vm);
    po::notify(vm);

    // check for help
    if (vm.count("help") || argc == 1)
    {
        cout << desc << endl;
        return 1;
    }

    // extract the options
    string game = vm["game"].as<string>();
    string board = vm.count("board") ? vm["board"].as<string>() : "";
    string hand = vm["hand"].as<string>();

    // allocate evaluator
    std::shared_ptr<PokerHandEvaluator> evaluator = PokerHandEvaluator::alloc(game);

    CardSet handCards = CardSet(hand);
    CardSet boardCards = CardSet(board);
    PokerHandEvaluation eval = evaluator->evaluateHand(handCards, boardCards);

    if (!eval.highlow())
    {
        cout << eval.high().toStringCannon() << ": " << eval.high().showdownCode() << endl;
    }
    else
    {
        cout << eval.high().toStringCannon() << ": " << eval.high().showdownCode() << endl;
        cout << eval.low().toStringCannon() << ": " << eval.low().showdownCode() << endl;
    }
}
