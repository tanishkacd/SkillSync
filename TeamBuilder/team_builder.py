# SkillSync - Automated Team Builder
# Person 5: Matching Engine
# Ranks candidates using proficiency, experience and availability.

candidates = [
    {
        "name": "Rahul",
        "skill": "Java",
        "proficiency": 5,
        "experience": 4,
        "availability": 80
    },
    {
        "name": "Priya",
        "skill": "Java",
        "proficiency": 4,
        "experience": 3,
        "availability": 50
    },
    {
        "name": "Amit",
        "skill": "Java",
        "proficiency": 2,
        "experience": 5,
        "availability": 100
    },
    {
        "name": "Sneha",
        "skill": "Java",
        "proficiency": 5,
        "experience": 1,
        "availability": 70
    },
    {
        "name": "Riya",
        "skill": "Java",
        "proficiency": 4,
        "experience": 4,
        "availability": 0
    },
    {
        "name": "Vikram",
        "skill": "Python",
        "proficiency": 5,
        "experience": 5,
        "availability": 90
    }
]


# Project requirements
required_skill = "Java"
minimum_proficiency = 3
minimum_experience = 2
required_headcount = 2


def calculate_match_score(candidate):
    """
    Match score:
    50% proficiency
    30% experience
    20% availability
    """

    proficiency_score = (candidate["proficiency"] / 5) * 50

    # Experience score capped at 5 years
    experience_score = (min(candidate["experience"], 5) / 5) * 30

    availability_score = (candidate["availability"] / 100) * 20

    total_score = (
        proficiency_score
        + experience_score
        + availability_score
    )

    return round(total_score, 2)


def find_candidates():
    eligible_candidates = []
    excluded_candidates = []

    for candidate in candidates:

        # Check skill
        if candidate["skill"].lower() != required_skill.lower():
            excluded_candidates.append(
                (candidate["name"], "Skill does not match")
            )
            continue

        # Check proficiency
        if candidate["proficiency"] < minimum_proficiency:
            excluded_candidates.append(
                (candidate["name"], "Underqualified - low proficiency")
            )
            continue

        # Check experience
        if candidate["experience"] < minimum_experience:
            excluded_candidates.append(
                (candidate["name"], "Underqualified - insufficient experience")
            )
            continue

        # Check availability
        if candidate["availability"] <= 0:
            excluded_candidates.append(
                (candidate["name"], "Fully booked")
            )
            continue

        # Candidate passed all checks
        candidate["match_score"] = calculate_match_score(candidate)
        eligible_candidates.append(candidate)

    # Highest score first
    eligible_candidates.sort(
        key=lambda candidate: candidate["match_score"],
        reverse=True
    )

    return eligible_candidates, excluded_candidates


def display_results():
    eligible, excluded = find_candidates()

    print("\n" + "=" * 55)
    print("        SKILLSYNC - AUTOMATED TEAM BUILDER")
    print("=" * 55)

    print("\nPROJECT REQUIREMENTS")
    print("-" * 55)
    print(f"Required Skill       : {required_skill}")
    print(f"Minimum Proficiency  : {minimum_proficiency}/5")
    print(f"Minimum Experience   : {minimum_experience} years")
    print(f"Required Headcount   : {required_headcount}")

    print("\nRANKED CANDIDATES")
    print("-" * 55)

    for index, candidate in enumerate(
        eligible[:required_headcount], start=1
    ):
        print(f"\n{index}. {candidate['name']}")
        print(f"   Skill         : {candidate['skill']}")
        print(f"   Proficiency   : {candidate['proficiency']}/5")
        print(f"   Experience    : {candidate['experience']} years")
        print(f"   Availability  : {candidate['availability']}%")
        print(f"   Match Score   : {candidate['match_score']}%")

    print("\nEXCLUDED CANDIDATES")
    print("-" * 55)

    for name, reason in excluded:
        print(f"✗ {name} -> {reason}")

    print("\nRECOMMENDED TEAM")
    print("-" * 55)

    selected = eligible[:required_headcount]

    if len(selected) < required_headcount:
        print(
            f"Only {len(selected)} suitable candidate(s) "
            f"found for {required_headcount} required."
        )
    else:
        for candidate in selected:
            print(f"✓ {candidate['name']}")

    print("\n" + "=" * 55)


# Run the Team Builder
if __name__ == "__main__":
    display_results()