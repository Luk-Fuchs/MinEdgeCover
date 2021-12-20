import matplotlib.pyplot as plt
import numpy as np
import itertools
import mip  
import time
import math


def generate_intervals(amount):
    res = []
    for i in range(amount):
        a , b = np.random.random(), np.random.random()
        for j in range(3):
            if np.abs(a-b)>0.2 or np.abs(a-b)<0.01:
                a , b = np.random.random(), np.random.random()
        res.append([a,b] if a< b else [b,a])
    return res
def generate_unit_intervals(amount, size = 0.2):
    res = []
    for i in range(amount):
        a = np.random.random()
        b= a+size
        res.append([a,b])
    return res
def findsubsets(S,m):
    return set(itertools.combinations(S, m))
def list_contains_list(x,to_search):
    for y in to_search:
        res=True
        if len(x)!=len(y):
            continue
        for i,_ in enumerate(x):
            if y[i]!=x[i]:
                res = False
        if res:
            return True
    return False
def valide_edge(edge):
    for i in range(len(edge)-1):
        for j in range(i+1,len(edge)):
            if edge[i][0]<=edge[j][0] and edge[i][1]>=edge[j][0] or edge[i][0]<=edge[j][1] and edge[i][1]>=edge[j][1]:
                return False
            tmp = i
            i=j
            j= tmp
            if edge[i][0]<=edge[j][0] and edge[i][1]>=edge[j][0] or edge[i][0]<=edge[j][1] and edge[i][1]>=edge[j][1]:
                return False
            tmp = i
            i=j
            j= tmp
    return True
def overlap(interval1, interval2):
    if interval1[0]<=interval2[0] and interval1[1]>=interval2[0] or interval1[0]<=interval2[1] and interval1[1]>=interval2[1]:
        return True
    if interval2[0]<=interval1[0] and interval2[1]>=interval1[0] or interval2[0]<=interval1[1] and interval2[1]>=interval1[1]:
        return True
    return False
def r(seed):
    np.random.seed(round(seed*100))
#     return (np.random.randint(0,255),np.random.randint(0,255),np.random.randint(0,255))
    return (np.random.rand(),np.random.rand(),np.random.rand(),1) 
def calc_max_cut(data):
    cut_points = [_[0]for _ in data]+[_[1] for _ in data]
    cut_points=sorted(cut_points)
    max_cut = 0
    max_pos=-1
    for point in cut_points:
        tmp_cuts = 0
        for interval in data:
            if interval[0] <=point and point <=interval[1]:
                tmp_cuts+=1
        if max_cut < tmp_cuts:
            max_cut = tmp_cuts
            max_pos = point
    return [max_cut, max_pos]
def MIP(data):
    intervals = list(data)
    model=mip.Model()
    
    #Variablen
    edges_3 = findsubsets([tuple(_) for _ in intervals],3)
    edges_3 = list(filter( lambda _ : valide_edge(_), edges_3))
    
#     edges_4 = findsubsets([tuple(_) for _ in intervals],4)
#     edges_4 = list(filter( lambda _ : valide_edge(_), edges_4))
    
    edges_2 = findsubsets([tuple(_) for _ in intervals],2)
    edges_2 = list(filter( lambda _ : valide_edge(_), edges_2))
    all_edges = edges_2 +  [[_] for _ in intervals] + edges_3 #+ edges_4
    
    x = [model.add_var(var_type=mip.BINARY) for _ in all_edges]
    
    
    model.objective = mip.minimize(mip.xsum(x))
    
    for interval in intervals: 
        model += mip.xsum( x[i] for i,e in enumerate(all_edges) if list_contains_list(interval,e)) == 1
        model += mip.xsum( x[i] for i,e in enumerate(all_edges) if list_contains_list(interval,e)) == 1
        
    model.optimize()
    return [_ for i,_ in enumerate(all_edges) if x[i].x==1 ]
def plot_matching(matching, draw_vertical_lines =True):
    plt.figure()
    for i,x in enumerate(matching):
        for y in x:
            plt.plot(y,[i,i],color=r(y[0]))
    if draw_vertical_lines:
        for i,x in enumerate(matching):
            for y in x:
                plt.plot([y[0],y[0]],[0,len(matching)],"--",color=(0.1,0.1,0.1,0.03))
                plt.plot([y[1],y[1]],[0,len(matching)],"--",color=(0.1,0.1,0.1,0.03))
    plt.show()
def try_reduce_problem_case_data(data, solver1, solver2, trys=100):
    intervals = list(data)
    print("initial length:", len(data))
    for i in range(trys):
#         print(intervals)
        delete_index = np.random.randint(0,len(intervals))
        tmp = [_ for i, _ in enumerate(intervals) if i !=delete_index]
#         print(tmp)
        if(len(solver1(tmp))!=len(solver2(tmp))):
            intervals=tmp
    print("length after processing",len(intervals))
    return intervals
def precalculate(data):
    result =[]
    while True:
        x = find_not_matchable_interval(data)
        if x != None:
            result.append([x])
            data.remove(x)
        else:
            x = find_only_once_matchable(data)
            if(x==None):
                break
            else:
                result.append(x)
                data.remove(x[0])
                data.remove(x[1])
    return result
def find_not_matchable_interval(data):
    for a in data:
        is_matchable = False
        for b in data:
            if a==b:
                continue
            if not overlap(a,b):
                is_matchable = True
        if not is_matchable:
            return a
    return None
def find_only_once_matchable(data):
    
    maxCount = len(data)
    possible_2_matching = None
    
    for a in data:
        #print("run=", a)
        is_3_matchable = False
        for b in data:
            for c in data:
                if not (overlap(a,b) or overlap(a,c) or overlap(b,c)):
                    is_3_matchable = True
                    break
        if is_3_matchable:
            continue
        for b in data:
            if not overlap(a,b):
                if a[0]>b[0]:
                    res1= b
                    res2 = a
                else:
                    res1=a
                    res2=b
                if possible_2_matching ==None:
                    possible_2_matching = [res1,res2]
                if res2[0]-res1[1]<possible_2_matching[1][0]-possible_2_matching[0][1]:
                    possible_2_matching=[res1,res2]
        #print("tmp_res=", possible_2_matching)
    return possible_2_matching
def find_next_matching_edge(data):
    max_cut_size, max_point = calc_max_cut(data)
    for e in data:
        if e[0] == max_point:
            edge = e
            break
    data.remove(edge)
    
    max_cut_size2, max_point2 = calc_max_cut(data)
    if max_cut_size2< max_cut_size:
        ##experimentell-----------
        for a in data:
            for b in data:
                if not overlap(a,b) and not overlap(edge,a) and not overlap(edge,b):
                    data.remove(a)
                    data.remove(b)
                    return [a,b,edge]
    else:
        for e in data:
            if e[0] == max_point2:
                edge2 = e
                break
        data.remove(edge2)
        
        max_cut_size3, max_point3 = calc_max_cut(data)
        if max_cut_size3< max_cut_size:
        ##fehlt noch-----------
            for a in data:
                    if not overlap(a,edge2) and not overlap(edge,a) and not overlap(edge,edge2):
                        data.remove(a)
                        return [a,edge,edge2]
        else:
            for e in data:
                if e[0] == max_point3:
                    edge3 = e
                    break
            data.remove(edge3)
            return[edge,edge2,edge3]
def solve(data_):
    data = list(data_)
    res =[]
    
    while(data):
        tmp =precalculate(data)
        res += tmp
        if(tmp):
            print("edge is added by precalc", tmp)
        if(not data):
            break
        tmp = [find_next_matching_edge(data)]   #Adds invalide edges
        res += tmp
        print("by alternative", tmp)
    return res
def greedy_solve_with_arbitrary_large_tuples(data):
    sorted_data = sorted(data,key=lambda _: _[0])
    res=[]
    for to_schedule in sorted_data:    
        possible_umläufe = list(filter(lambda _:_[-1][1]<to_schedule[0],res))
        if possible_umläufe:
            sorted(possible_umläufe,key=lambda _: -_[-1][1])[0].append(to_schedule)
        else:
            res.append([to_schedule])
    return res
data = generate_intervals(20)
        
matching = greedy_solve_with_arbitrary_large_tuples(data)
plot_matching(matching)