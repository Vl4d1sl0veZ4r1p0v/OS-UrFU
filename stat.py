from itertools import permutations


with open('data.txt') as hfile:
    buffer = hfile.read().split('\n')

count = []
wait_time = []
for line in buffer:
    line = line.split(' ')

    count.append(int(line[1]))
    wait_time.append(int(line[2]))

count_median = 0
wait_median = 0
for i in count:
    count_median += i
for i in wait_time:
    wait_median += i

print("count av: " + str(int(count_median / len(count))))
print("wait av: " + str(int(wait_median / len(wait_time))))

match_counts = []
current_match = []
for line in buffer:
    line = line.split(' ')
    if line[0] == '1':
         match_counts.append(current_match)
         current_match = [int(line[1])]
         continue

    current_match.append(int(line[1]))

match_counts.append(current_match)
match_counts = match_counts[1:]

global_diffs = []
for i in range(len(match_counts)):
     match_diffs = []
     for data in permutations(match_counts[i], 2):
         match_diffs.append(abs(data[0] - data[1]))
     global_diffs.append(match_diffs)

global_av_diffs = []
global_max_diff = []
for i in range(len(global_diffs)):
    match_av_diff = 0
    match_max_diff = 0
    for diff in global_diffs[i]:
        match_av_diff += diff
        if match_max_diff < diff:
             match_max_diff = diff

    match_av_diff = int(match_av_diff / len(global_diffs[i]))
    global_av_diffs.append(match_av_diff)
    global_max_diff.append(match_max_diff)

final_global_av_diff = 0
for i in global_av_diffs:
    final_global_av_diff += i
final_global_av_diff = int(final_global_av_diff / len(global_av_diffs))
print("average diff: " + str(final_global_av_diff))

final_global_max_diff = 0
for i in global_max_diff:
    if final_global_max_diff < i:
        final_global_max_diff = i
print("max diff: " + str(final_global_max_diff))
